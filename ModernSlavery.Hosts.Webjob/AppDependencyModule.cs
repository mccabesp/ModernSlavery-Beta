using Autofac;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.BusinessLogic;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel.Extensions;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.Hosts.Webjob.Classes;
using ModernSlavery.Hosts.Webjob.Jobs;
using ModernSlavery.Infrastructure.CompaniesHouse;
using ModernSlavery.Infrastructure.Database;
using ModernSlavery.Infrastructure.Logging;
using ModernSlavery.Infrastructure.Messaging;
using ModernSlavery.Infrastructure.Search;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.WebUI.Shared.Options;
using DataProtectionOptions = Microsoft.AspNetCore.DataProtection.DataProtectionOptions;
using ResponseCachingOptions = Microsoft.AspNetCore.ResponseCaching.ResponseCachingOptions;

namespace ModernSlavery.Hosts.Webjob
{
    public class AppDependencyModule: IDependencyModule
    {
        private readonly SharedOptions _sharedOptions;
        private readonly CompaniesHouseOptions _coHoOptions;
        private readonly ResponseCachingOptions _responseCachingOptions;
        private readonly DistributedCacheOptions _distributedCacheOptions;
        private readonly DataProtectionOptions _dataProtectionOptions;

        public AppDependencyModule(SharedOptions sharedOptions, CompaniesHouseOptions coHoOptions, ResponseCachingOptions responseCachingOptions, DistributedCacheOptions distributedCacheOptions, DataProtectionOptions dataProtectionOptions)
        {
            _sharedOptions = sharedOptions;
            _coHoOptions = coHoOptions;
            _responseCachingOptions = responseCachingOptions;
            _distributedCacheOptions = distributedCacheOptions;
            _dataProtectionOptions = dataProtectionOptions;
        }

        public void Bind(ContainerBuilder builder, IServiceCollection services)
        {
            services.AddHttpClient<GovNotifyEmailProvider>(nameof(GovNotifyEmailProvider));

            services.AddApplicationInsightsTelemetry(_sharedOptions.APPINSIGHTS_INSTRUMENTATIONKEY);

            services.AddSingleton<IJobActivator, AutofacJobActivator>();

            //Register the database dependencies
            builder.RegisterDependencyModule<DatabaseDependencyModule>();

            //Register the file storage dependencies
            builder.RegisterDependencyModule<FileStorageDependencyModule>();

            //Register the log storage dependencies
            builder.RegisterDependencyModule<LoggingDependencyModule>();

            //Register the search dependencies
            builder.RegisterDependencyModule<SearchDependencyModule>();

            //Register the companies house dependencies
            builder.RegisterDependencyModule<CompaniesHouseDependencyModule>();

            //Register the messaging dependencies
            builder.RegisterType<Messenger>().As<IMessenger>().SingleInstance();
            builder.RegisterType<GovNotifyAPI>().As<IGovNotifyAPI>().SingleInstance();

            // Register the email template dependencies
            builder.RegisterInstance(new EmailTemplateRepository(FileSystem.ExpandLocalPath("~/App_Data/EmailTemplates"))).As<IEmailTemplateRepository>().SingleInstance();

            //Register some singletons
            builder.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance().WithParameter("seed", _sharedOptions.ObfuscationSeed);
            builder.RegisterType<EncryptionHandler>().As<IEncryptionHandler>().SingleInstance();

            // Register email provider dependencies
            builder.RegisterType<GovNotifyEmailProvider>().SingleInstance();
            builder.RegisterType<SmtpEmailProvider>().SingleInstance();
            builder.RegisterType<EmailProvider>().SingleInstance();

            #region Register the busines logic dependencies
            builder.RegisterType<SharedBusinessLogic>().As<ISharedBusinessLogic>().SingleInstance();
            builder.RegisterType<ScopeBusinessLogic>().As<IScopeBusinessLogic>().InstancePerDependency();
            builder.RegisterType<SubmissionBusinessLogic>().As<ISubmissionBusinessLogic>().InstancePerDependency();
            builder.RegisterType<SecurityCodeBusinessLogic>().As<ISecurityCodeBusinessLogic>().InstancePerDependency();
            builder.RegisterType<OrganisationBusinessLogic>().As<IOrganisationBusinessLogic>().InstancePerDependency();
            builder.RegisterType<SearchBusinessLogic>().As<ISearchBusinessLogic>().SingleInstance();
            builder.RegisterType<UpdateFromCompaniesHouseService>().As<UpdateFromCompaniesHouseService>().InstancePerDependency();
            #endregion

            // Need to register webJob class in Autofac as well
            builder.RegisterType<Functions>().InstancePerDependency();
            builder.RegisterType<DisableWebjobProvider>().SingleInstance();

        }
    }
}
