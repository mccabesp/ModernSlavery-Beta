using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.EmailTemplates;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.Hosts.Webjob.Classes;
using ModernSlavery.Hosts.Webjob.Jobs;
using ModernSlavery.Infrastructure.CompaniesHouse;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.Infrastructure.Messaging;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.WebUI.Shared.Options;
using DataProtectionOptions = Microsoft.AspNetCore.DataProtection.DataProtectionOptions;
using ResponseCachingOptions = Microsoft.AspNetCore.ResponseCaching.ResponseCachingOptions;

namespace ModernSlavery.Hosts.Webjob
{
    public class DependencyModule : IDependencyModule
    {
        private readonly ILogger _logger;
        private readonly CompaniesHouseOptions _coHoOptions;
        private readonly DataProtectionOptions _dataProtectionOptions;
        private readonly DistributedCacheOptions _distributedCacheOptions;
        private readonly ResponseCachingOptions _responseCachingOptions;
        private readonly SharedOptions _sharedOptions;

        public DependencyModule(
            ILogger<DependencyModule> logger, 
            SharedOptions sharedOptions, CompaniesHouseOptions coHoOptions,
            ResponseCachingOptions responseCachingOptions, DistributedCacheOptions distributedCacheOptions,
            DataProtectionOptions dataProtectionOptions)
        {
            _logger = logger;
            _sharedOptions = sharedOptions;
            _coHoOptions = coHoOptions;
            _responseCachingOptions = responseCachingOptions;
            _distributedCacheOptions = distributedCacheOptions;
            _dataProtectionOptions = dataProtectionOptions;
        }

        public void Register(IDependencyBuilder builder)
        {
            builder.Services.AddHttpClient<GovNotifyEmailProvider>(nameof(GovNotifyEmailProvider));

            builder.Services.AddApplicationInsightsTelemetry(_sharedOptions.AppInsights_InstrumentationKey);

            builder.Services.AddSingleton<IJobActivator, AutofacJobActivator>();

            //Register the file storage dependencies
            builder.RegisterModule<FileStorageDependencyModule>();

            //Register the log storage dependencies
            builder.RegisterModule<Infrastructure.Logging.DependencyModule>();

            //Register the messaging dependencies
            builder.Autofac.RegisterType<Messenger>().As<IMessenger>().SingleInstance();
            builder.Autofac.RegisterType<GovNotifyAPI>().As<IGovNotifyAPI>().SingleInstance();

            // Register the email template dependencies
            builder.Autofac
                .RegisterInstance(new EmailTemplateRepository(FileSystem.ExpandLocalPath("~/App_Data/EmailTemplates")))
                .As<IEmailTemplateRepository>().SingleInstance();

            //Register some singletons
            builder.Autofac.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance()
                .WithParameter("seed", _sharedOptions.ObfuscationSeed);
            builder.Autofac.RegisterType<EncryptionHandler>().As<IEncryptionHandler>().SingleInstance();

            // Register email provider dependencies
            builder.Autofac.RegisterType<GovNotifyEmailProvider>().SingleInstance().WithAttributeFiltering();
            builder.Autofac.RegisterType<SmtpEmailProvider>().SingleInstance().WithAttributeFiltering();
            builder.Autofac.RegisterType<EmailProvider>().SingleInstance().WithAttributeFiltering();

            // Need to register webJob class in Autofac as well
            builder.Autofac.RegisterType<Functions>().InstancePerDependency();
            builder.Autofac.RegisterType<DisableWebjobProvider>().SingleInstance();

        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //Add configuration here
            var app = lifetimeScope.Resolve<IWebJobsBuilder>();

            var lifetime = lifetimeScope.Resolve<IHostApplicationLifetime>(); 
            var config = lifetimeScope.Resolve<IConfiguration>();
            var fileRepository = lifetimeScope.Resolve<IFileRepository>();
            var sharedOptions = lifetimeScope.Resolve<SharedOptions>();

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

            void RegisterEmailTemplate<TTemplate>(string templatesConfigPath) where TTemplate : EmailTemplate
            {
                // resolve config and resolve template repository
                var repo = lifetimeScope.Resolve<IEmailTemplateRepository>();

                // get the template id using the type name
                var templateConfigKey = typeof(TTemplate).Name;
                var templateId = config.GetValue<string>($"{templatesConfigPath}:{templateConfigKey}");

                // add this template to the repository
                repo.Add<TTemplate>(templateId, $"{templateConfigKey}.html");
            }

            Task.WaitAll(fileRepository.PushRemoteFileAsync(Filenames.SicSectorSynonyms, sharedOptions.DataPath));

            lifetime.ApplicationStarted.Register(
                () =>
                {
                    // Summary:
                    //     Triggered when the application host has fully started and is about to wait for
                    //     a graceful shutdown.
                    _logger.LogInformation("Application Started");
                });
            lifetime.ApplicationStopping.Register(
                () =>
                {
                    // Summary:
                    //     Triggered when the application host is performing a graceful shutdown. Requests
                    //     may still be in flight. Shutdown will block until this event completes.
                    _logger.LogInformation("Application Stopping");
                });
        }

        public class AutofacJobActivator : IJobActivator
        {
            private readonly IContainer _container;

            public AutofacJobActivator(IContainer container)
            {
                _container = container;
            }

            public T CreateInstance<T>()
            {
                return _container.Resolve<T>();
            }
        }
    }
}