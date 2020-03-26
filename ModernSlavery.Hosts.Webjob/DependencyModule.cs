using System;
using Autofac;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.BusinessDomain.Admin;
using ModernSlavery.BusinessDomain.Registration;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.BusinessDomain.Viewing;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.Hosts.Webjob.Classes;
using ModernSlavery.Hosts.Webjob.Jobs;
using ModernSlavery.Infrastructure.CompaniesHouse;
using ModernSlavery.Infrastructure.Database;
using ModernSlavery.Infrastructure.Messaging;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.WebUI.Shared.Options;
using DataProtectionOptions = Microsoft.AspNetCore.DataProtection.DataProtectionOptions;
using ResponseCachingOptions = Microsoft.AspNetCore.ResponseCaching.ResponseCachingOptions;

namespace ModernSlavery.Hosts.Webjob
{
    public class DependencyModule : IDependencyModule
    {
        private readonly CompaniesHouseOptions _coHoOptions;
        private readonly DataProtectionOptions _dataProtectionOptions;
        private readonly DistributedCacheOptions _distributedCacheOptions;
        private readonly ResponseCachingOptions _responseCachingOptions;
        private readonly SharedOptions _sharedOptions;

        public DependencyModule(SharedOptions sharedOptions, CompaniesHouseOptions coHoOptions,
            ResponseCachingOptions responseCachingOptions, DistributedCacheOptions distributedCacheOptions,
            DataProtectionOptions dataProtectionOptions)
        {
            _sharedOptions = sharedOptions;
            _coHoOptions = coHoOptions;
            _responseCachingOptions = responseCachingOptions;
            _distributedCacheOptions = distributedCacheOptions;
            _dataProtectionOptions = dataProtectionOptions;
        }

        public bool AutoSetup { get; } = false;

        public void Register(IDependencyBuilder builder)
        {
            builder.ServiceCollection.AddHttpClient<GovNotifyEmailProvider>(nameof(GovNotifyEmailProvider));

            builder.ServiceCollection.AddApplicationInsightsTelemetry(_sharedOptions.AppInsights_InstrumentationKey);

            builder.ServiceCollection.AddSingleton<IJobActivator, AutofacJobActivator>();

            //Register the database dependencies
            builder.RegisterModule<DatabaseDependencyModule>();

            //Register the file storage dependencies
            builder.RegisterModule<FileStorageDependencyModule>();

            //Register the log storage dependencies
            builder.RegisterModule<Infrastructure.Logging.DependencyModule>();

            //Register the search dependencies
            builder.RegisterModule<Infrastructure.Search.DependencyModule>();

            //Register the companies house dependencies
            builder.RegisterModule<Infrastructure.CompaniesHouse.DependencyModule>();

            //Register the messaging dependencies
            builder.ContainerBuilder.RegisterType<Messenger>().As<IMessenger>().SingleInstance();
            builder.ContainerBuilder.RegisterType<GovNotifyAPI>().As<IGovNotifyAPI>().SingleInstance();

            // Register the email template dependencies
            builder.ContainerBuilder
                .RegisterInstance(new EmailTemplateRepository(FileSystem.ExpandLocalPath("~/App_Data/EmailTemplates")))
                .As<IEmailTemplateRepository>().SingleInstance();

            //Register some singletons
            builder.ContainerBuilder.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance()
                .WithParameter("seed", _sharedOptions.ObfuscationSeed);
            builder.ContainerBuilder.RegisterType<EncryptionHandler>().As<IEncryptionHandler>().SingleInstance();

            // Register email provider dependencies
            builder.ContainerBuilder.RegisterType<GovNotifyEmailProvider>().SingleInstance();
            builder.ContainerBuilder.RegisterType<SmtpEmailProvider>().SingleInstance();
            builder.ContainerBuilder.RegisterType<EmailProvider>().SingleInstance();

            #region Register the busines logic dependencies

            builder.ContainerBuilder.RegisterType<SharedBusinessLogic>().As<ISharedBusinessLogic>().SingleInstance();
            builder.ContainerBuilder.RegisterType<ScopeBusinessLogic>().As<IScopeBusinessLogic>()
                .InstancePerDependency();
            builder.ContainerBuilder.RegisterType<SubmissionBusinessLogic>().As<ISubmissionBusinessLogic>()
                .InstancePerDependency();
            builder.ContainerBuilder.RegisterType<SecurityCodeBusinessLogic>().As<ISecurityCodeBusinessLogic>()
                .InstancePerDependency();
            builder.ContainerBuilder.RegisterType<OrganisationBusinessLogic>().As<IOrganisationBusinessLogic>()
                .InstancePerDependency();
            builder.ContainerBuilder.RegisterType<SearchBusinessLogic>().As<ISearchBusinessLogic>().SingleInstance();
            builder.ContainerBuilder.RegisterType<UpdateFromCompaniesHouseService>()
                .As<UpdateFromCompaniesHouseService>().InstancePerDependency();

            #endregion

            // Need to register webJob class in Autofac as well
            builder.ContainerBuilder.RegisterType<Functions>().InstancePerDependency();
            builder.ContainerBuilder.RegisterType<DisableWebjobProvider>().SingleInstance();
        }

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
            //TODO: Add configuration here
        }
    }
}