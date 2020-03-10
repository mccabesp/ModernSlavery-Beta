using System;
using System.Net.Http;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using ModernSlavery.BusinessLogic;
using ModernSlavery.BusinessLogic.Services;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.SharedKernel;
using ModernSlavery.Extensions;
using ModernSlavery.Extensions.AspNetCore;
using Microsoft.Azure.Search;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using ModernSlavery.SharedKernel.Interfaces;
using ModernSlavery.Database;
using ModernSlavery.Infrastructure;
using ModernSlavery.Infrastructure.Data;
using ModernSlavery.Infrastructure.File;
using ModernSlavery.Infrastructure.Logging;
using ModernSlavery.Infrastructure.Message;
using ModernSlavery.Infrastructure.Queue;
using ModernSlavery.Infrastructure.Search;

namespace ModernSlavery.WebJob
{
    public class Startup
    {

        // ConfigureServices is where you register dependencies. This gets
        // called by the runtime before the ConfigureContainer method, below.
        public static void ConfigureServices(IServiceCollection services)
        {
            var coHoOptions = new CompaniesHouseOptions();
            Config.Configuration.Bind("CompaniesHouse", coHoOptions);

            //Add a dedicated httpClient for Companies house API with exponential retry policy

            services.AddHttpClient<ICompaniesHouseAPI, CompaniesHouseAPI>(nameof(ICompaniesHouseAPI), (httpClient)=>
                {
                    CompaniesHouseAPI.SetupHttpClient(httpClient, coHoOptions.ApiServer, coHoOptions.ApiKey);
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(CompaniesHouseAPI.GetRetryPolicy());

            services.AddHttpClient<GovNotifyEmailProvider>(nameof(GovNotifyEmailProvider));

            // setup configuration options
            services.Configure<CompaniesHouseOptions>(Config.Configuration.GetSection("CompaniesHouse"));
            services.Configure<GovNotifyOptions>(Config.Configuration.GetSection("Email:Providers:GovNotify"));
            services.Configure<SmtpEmailOptions>(Config.Configuration.GetSection("Email:Providers:Smtp"));
            services.Configure<GpgEmailOptions>(Config.Configuration);

            //Create the Autofac inversion of control container
            var builder = new ContainerBuilder();

            //Populate autoAutofacfac container with any existing services registered already
            builder.Populate(services);

            //Configure the container
            BuildContainerIoC(builder);

            //Build the container
            Program.ContainerIOC = builder.Build();

            //Register Autofac as the service provider
            services.AddSingleton<IServiceProvider>(new AutofacServiceProvider(Program.ContainerIOC));

            //Register the webJobs IJobActivator
            services.AddSingleton(Program.ContainerIOC);
            services.AddSingleton<IJobActivator, AutofacJobActivator>();
        }

        public static void BuildContainerIoC(ContainerBuilder builder)
        {
            // Need to register webJob class in Autofac as well
            builder.RegisterType<Functions>().InstancePerDependency();
            builder.RegisterType<DisableWebjobProvider>().SingleInstance();

            builder.Register(c => new SqlRepository(new DatabaseContext(Global.DatabaseConnectionString)))
                .As<IDataRepository>()
                .InstancePerDependency();
            builder.RegisterType<CompaniesHouseAPI>()
                .As<ICompaniesHouseAPI>()
                .SingleInstance()
                .WithParameter(
                    (p, ctx) => p.ParameterType == typeof(HttpClient),
                    (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient(nameof(ICompaniesHouseAPI)));

            // validate we have a storage connection
            if (string.IsNullOrWhiteSpace(Global.AzureStorageConnectionString))
            {
                throw new InvalidOperationException("No Azure Storage connection specified. Check the config.");
            }

            //Set the default encryption key
            Encryption.SetDefaultEncryptionKey(Config.GetAppSetting("DefaultEncryptionKey"));

            string azureStorageShareName = Config.GetAppSetting("AzureStorageShareName");
            string localStorageRoot = Config.GetAppSetting("LocalStorageRoot");

            if (string.IsNullOrWhiteSpace(localStorageRoot))
            {
                //Exponential retry policy is recommended for background tasks - see https://docs.microsoft.com/en-us/azure/architecture/best-practices/retry-service-specific#azure-storage
                builder.Register(
                        c => new AzureFileRepository(
                            Global.AzureStorageConnectionString,
                            azureStorageShareName,
                            new ExponentialRetry(TimeSpan.FromSeconds(3), 10)))
                    .As<IFileRepository>()
                    .SingleInstance();
            }
            else
            {
                builder.Register(c => new SystemFileRepository(localStorageRoot)).As<IFileRepository>().SingleInstance();
            }

            builder.RegisterType<Messenger>().As<IMessenger>().SingleInstance();
            builder.RegisterType<GovNotifyAPI>().As<IGovNotifyAPI>().SingleInstance();

            // Setup azure search
            var azureSearchServiceName = Config.GetAppSetting("SearchService:ServiceName");
            //var azureSearchQueryKey = Config.GetAppSetting("SearchService:QueryApiKey");
            var azureSearchAdminKey = Config.GetAppSetting("SearchService:AdminApiKey");
            var azureSearchDisabled = Config.GetAppSetting("SearchService:Disabled").ToBoolean();

            builder.Register(c => new SearchServiceClient(azureSearchServiceName, new SearchCredentials(azureSearchAdminKey)))
                .As<ISearchServiceClient>()
                .SingleInstance();

            builder.RegisterType<AzureEmployerSearchRepository>()
                .As<ISearchRepository<EmployerSearchModel>>()
                .SingleInstance()
                .WithParameter("serviceName", azureSearchServiceName)
                .WithParameter("adminApiKey", azureSearchAdminKey)
                .WithParameter("disabled", azureSearchDisabled);

            builder.RegisterType<AzureSicCodeSearchRepository>()
                .As<ISearchRepository<SicCodeSearchModel>>()
                .SingleInstance()
                .WithParameter("disabled", azureSearchDisabled);

            builder.RegisterInstance(new EmailTemplateRepository(FileSystem.ExpandLocalPath("~/App_Data/EmailTemplates")))
                .As<IEmailTemplateRepository>()
                .SingleInstance();

            // BL Services
            builder.RegisterInstance(Config.Configuration).SingleInstance();
            builder.RegisterType<CommonBusinessLogic>().As<ICommonBusinessLogic>().SingleInstance();
            builder.RegisterType<ScopeBusinessLogic>().As<IScopeBusinessLogic>().InstancePerDependency();
            builder.RegisterType<SubmissionBusinessLogic>().As<ISubmissionBusinessLogic>().InstancePerDependency();
            builder.RegisterType<SecurityCodeBusinessLogic>().As<ISecurityCodeBusinessLogic>().InstancePerDependency();
            builder.RegisterType<OrganisationBusinessLogic>().As<IOrganisationBusinessLogic>().InstancePerDependency();
            builder.RegisterType<SearchBusinessLogic>().As<ISearchBusinessLogic>().SingleInstance();
            builder.RegisterType<UpdateFromCompaniesHouseService>().As<UpdateFromCompaniesHouseService>().InstancePerDependency();

            //Register some singletons
            builder.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance();
            builder.RegisterType<EncryptionHandler>().As<IEncryptionHandler>().SingleInstance();

            // Register queues (without key filtering)
            builder.Register(c => new LogEventQueue(Global.AzureStorageConnectionString, c.Resolve<IFileRepository>())).SingleInstance();
            builder.Register(c => new LogRecordQueue(Global.AzureStorageConnectionString, c.Resolve<IFileRepository>())).SingleInstance();

            // Register record log queues
            builder.RegisterLogRecord(Filenames.EmailSendLog);
            builder.RegisterLogRecord(Filenames.StannpSendLog);
            builder.RegisterLogRecord(Filenames.ManualChangeLog);
            builder.RegisterLogRecord(Filenames.BadSicLog);


            // Register record log records (without key filtering)
            builder.RegisterType<UserLogRecord>().As<IUserLogRecord>().SingleInstance();

            // Register email providers
            builder.RegisterType<GovNotifyEmailProvider>().SingleInstance();
            builder.RegisterType<SmtpEmailProvider>().SingleInstance();
            builder.RegisterType<GpgEmailProvider>().SingleInstance();
        }

    }
}
