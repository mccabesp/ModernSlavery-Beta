using System;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using ModernSlavery.BusinessLogic;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.SharedKernel;
using ModernSlavery.Extensions;
using Microsoft.Azure.Search;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using ModernSlavery.SharedKernel.Interfaces;
using ModernSlavery.Database;
using ModernSlavery.Infrastructure;
using ModernSlavery.Infrastructure.Data;
using ModernSlavery.Infrastructure.Logging;
using ModernSlavery.Infrastructure.Message;
using ModernSlavery.Infrastructure.Queue;
using ModernSlavery.Infrastructure.Search;
using ModernSlavery.SharedKernel.Options;
using ModernSlavery.Core.EmailTemplates;
using ModernSlavery.Infrastructure.Configuration;
using ModernSlavery.Infrastructure.File;
using ModernSlavery.Infrastructure.Options;

namespace ModernSlavery.WebJob
{
    public class Startup:IStartup
    {
        private readonly IConfiguration _Config;
        private readonly ILogger _Logger;
        private IServiceProvider _ServiceProvider;
        private OptionsBinder OptionsBinder;

        public Startup(IConfiguration config)
        {
            _Config = config;
            _Logger = Activator.CreateInstance<Logger<Startup>>();
        }

        // ConfigureServices is where you register dependencies. This gets
        // called by the runtime before the ConfigureContainer method, below.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            #region Bind the options classes and register as services
            OptionsBinder = new OptionsBinder(services, _Config);
            OptionsBinder.BindAssemblies("ModernSlavery");
            var globalOptions = OptionsBinder.Get<GlobalOptions>();
            var coHoOptions = OptionsBinder.Get<CompaniesHouseOptions>();
            #endregion

            //Add a dedicated httpClient for Companies house API with exponential retry policy
            services.AddHttpClient<ICompaniesHouseAPI, CompaniesHouseAPI>(nameof(ICompaniesHouseAPI), (httpClient)=>
                {
                    CompaniesHouseAPI.SetupHttpClient(httpClient, coHoOptions.ApiServer, coHoOptions.ApiKey);
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(CompaniesHouseAPI.GetRetryPolicy());

            services.AddHttpClient<GovNotifyEmailProvider>(nameof(GovNotifyEmailProvider));

            services.AddApplicationInsightsTelemetry(_Config.GetValue("ApplicationInsights:InstrumentationKey", _Config["APPINSIGHTS-INSTRUMENTATIONKEY"]));

            //Create the Autofac inversion of control container
            var builder = new ContainerBuilder();

            // Note that Populate is basically a foreach to add things
            // into Autofac that are in the collection. If you register
            // things in Autofac BEFORE Populate then the stuff in the
            // ServiceCollection can override those things; if you register
            // AFTER Populate those registrations can override things
            // in the ServiceCollection. Mix and match as needed.
            builder.Populate(services);

            //Configure the container
            var container = BuildContainer(builder);

            //Register Autofac as the service provider
            _ServiceProvider = new AutofacServiceProvider(container);
            services.AddSingleton(_ServiceProvider);

            //Register the webJobs IJobActivator
            services.AddSingleton(container);
            services.AddSingleton<IJobActivator, AutofacJobActivator>();

            return container.Resolve<IServiceProvider>();
        }

        private IContainer BuildContainer(ContainerBuilder builder)
        {
            var globalOptions = OptionsBinder.Get<GlobalOptions>();
            var databaseOptions = OptionsBinder.Get<DatabaseOptions>();
            var storageOptions = OptionsBinder.Get<StorageOptions>();
            var searchOptions = OptionsBinder.Get<SearchOptions>();

            // Need to register webJob class in Autofac as well
            builder.RegisterType<Functions>().InstancePerDependency();
            builder.RegisterType<DisableWebjobProvider>().SingleInstance();

            #region Database
            builder.Register(c => new SqlRepository(new DatabaseContext(globalOptions, databaseOptions)))
                .As<IDataRepository>()
                .InstancePerDependency();
            #endregion

            #region Companies House AP
            builder.RegisterType<CompaniesHouseAPI>()
                .As<ICompaniesHouseAPI>()
                .SingleInstance()
                .WithParameter(
                    (p, ctx) => p.ParameterType == typeof(HttpClient),
                    (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient(nameof(ICompaniesHouseAPI)));

            #endregion

            #region File Storage
            if (string.IsNullOrWhiteSpace(storageOptions.LocalStorageRoot))
            {
                //Exponential retry policy is recommended for background tasks - see https://docs.microsoft.com/en-us/azure/architecture/best-practices/retry-service-specific#azure-storage
                builder.Register(
                        c => new AzureFileRepository(storageOptions,new ExponentialRetry(TimeSpan.FromSeconds(3), 10)))
                    .As<IFileRepository>()
                    .SingleInstance();
            }
            else
            {
                builder.Register(c => new SystemFileRepository(storageOptions)).As<IFileRepository>().SingleInstance();
            }
            #endregion

            builder.RegisterType<Messenger>().As<IMessenger>().SingleInstance();
            builder.RegisterType<GovNotifyAPI>().As<IGovNotifyAPI>().SingleInstance();

            #region Search
            builder.Register(c => new SearchServiceClient(searchOptions.AzureServiceName, new SearchCredentials(searchOptions.AzureApiAdminKey)))
                .As<ISearchServiceClient>()
                .SingleInstance();

            builder.RegisterType<AzureEmployerSearchRepository>()
                .As<ISearchRepository<EmployerSearchModel>>()
                .SingleInstance()
                .WithParameter("serviceName", searchOptions.AzureServiceName)
                .WithParameter("adminApiKey", searchOptions.AzureApiAdminKey)
                .WithParameter("disabled", searchOptions.Disabled);

            builder.RegisterType<AzureSicCodeSearchRepository>()
                .As<ISearchRepository<SicCodeSearchModel>>()
                .SingleInstance()
                .WithParameter("disabled", searchOptions.Disabled);
            #endregion

            #region Email Templates
            builder.RegisterInstance(new EmailTemplateRepository(FileSystem.ExpandLocalPath("~/App_Data/EmailTemplates")))
                .As<IEmailTemplateRepository>()
                .SingleInstance();
            #endregion

            // BL Services
            builder.RegisterInstance(_Config).SingleInstance();

            #region Business Logic
            builder.RegisterType<CommonBusinessLogic>().As<ICommonBusinessLogic>().SingleInstance();
            builder.RegisterType<ScopeBusinessLogic>().As<IScopeBusinessLogic>().InstancePerDependency();
            builder.RegisterType<SubmissionBusinessLogic>().As<ISubmissionBusinessLogic>().InstancePerDependency();
            builder.RegisterType<SecurityCodeBusinessLogic>().As<ISecurityCodeBusinessLogic>().InstancePerDependency();
            builder.RegisterType<OrganisationBusinessLogic>().As<IOrganisationBusinessLogic>().InstancePerDependency();
            builder.RegisterType<SearchBusinessLogic>().As<ISearchBusinessLogic>().SingleInstance();
            builder.RegisterType<UpdateFromCompaniesHouseService>().As<UpdateFromCompaniesHouseService>().InstancePerDependency();
            #endregion

            //Register some singletons
            builder.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance().WithParameter("seed", globalOptions.ObfuscationSeed);
            builder.RegisterType<EncryptionHandler>().As<IEncryptionHandler>().SingleInstance();

            // Register queues (without key filtering)
            builder.Register(c => new LogEventQueue(storageOptions.AzureConnectionString, c.Resolve<IFileRepository>())).SingleInstance();
            builder.Register(c => new LogRecordQueue(storageOptions.AzureConnectionString, c.Resolve<IFileRepository>())).SingleInstance();

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
            builder.RegisterType<EmailProvider>().SingleInstance();

            //Build the container
            return builder.Build();
        }

        public void Configure(IApplicationBuilder app=null)
        {
            //Set the default encryption key
            Encryption.SetDefaultEncryptionKey(_Config["DefaultEncryptionKey"]);

            //Ensure SicSectorSynonyms exist on remote 

            var fileRepository = _ServiceProvider.GetService<IFileRepository>();
            var globalOptions = _ServiceProvider.GetService<GlobalOptions>();

            //Initialise the virtual date and time
            VirtualDateTime.Initialise(globalOptions.DateTimeOffset);

            Task.WaitAll(Core.Classes.Extensions.PushRemoteFileAsync(fileRepository, Filenames.SicSectorSynonyms, globalOptions.DataPath));

            // Register email templates
            var emailTemplatesConfigPath = "Email:Templates";
            // Gpg templates
            RegisterEmailTemplate<ChangeEmailPendingVerificationTemplate>(emailTemplatesConfigPath);
            RegisterEmailTemplate<ChangeEmailCompletedVerificationTemplate>(emailTemplatesConfigPath);
            RegisterEmailTemplate<ChangeEmailCompletedNotificationTemplate>(emailTemplatesConfigPath);
            RegisterEmailTemplate<ChangePasswordCompletedTemplate>(emailTemplatesConfigPath);
            RegisterEmailTemplate<ResetPasswordVerificationTemplate>(emailTemplatesConfigPath);
            RegisterEmailTemplate<ResetPasswordCompletedTemplate>(emailTemplatesConfigPath);
            RegisterEmailTemplate<CloseAccountCompletedTemplate>(emailTemplatesConfigPath);
            RegisterEmailTemplate<OrphanOrganisationTemplate>(emailTemplatesConfigPath);
            RegisterEmailTemplate<CreateAccountPendingVerificationTemplate>(emailTemplatesConfigPath);
            RegisterEmailTemplate<OrganisationRegistrationApprovedTemplate>(emailTemplatesConfigPath);
            RegisterEmailTemplate<OrganisationRegistrationDeclinedTemplate>(emailTemplatesConfigPath);
            RegisterEmailTemplate<OrganisationRegistrationRemovedTemplate>(emailTemplatesConfigPath);
            RegisterEmailTemplate<GeoOrganisationRegistrationRequestTemplate>(emailTemplatesConfigPath);

            // system templates
            RegisterEmailTemplate<SendEmailTemplate>(emailTemplatesConfigPath);
        }

        /// <summary>
        ///     Maps a template model to a corresponding entry in the appsetting config
        /// </summary>
        /// <example>
        ///     // appsettings.json example
        ///     {
        ///     "Email": {
        ///     "Templates": {
        ///     "MyTemplateName": "c97cb8d6-4b1b-468f-812e-af77e1f2422c"
        ///     }
        ///     }
        ///     }
        ///     // Email template example
        ///     public class MyTemplateName : ATemplate
        ///     {
        ///     // Merge fields used with Gov Notify or Smtp templates...
        ///     public string Field1 {get; set;}
        ///     public string Field2 {get; set;}
        ///     }
        ///     // usage example
        ///     host.RegisterEmailTemplate<MyTemplateName>("Email:Templates");
        /// </example>
        private void RegisterEmailTemplate<TTemplate>(string templatesConfigPath) where TTemplate : EmailTemplate
        {
            // resolve config and resolve template repository
            var repo = _ServiceProvider.GetService<IEmailTemplateRepository>();

            // get the template id using the type name
            string templateConfigKey = typeof(TTemplate).Name;
            var templateId = _Config.GetValue<string>($"{templatesConfigPath}:{templateConfigKey}");

            // add this template to the repository
            repo.Add<TTemplate>(templateId, $"{templateConfigKey}.html");
        }
    }
}
