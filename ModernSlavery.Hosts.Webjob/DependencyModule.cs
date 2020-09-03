using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.EmailTemplates;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Hosts.Webjob.Classes;
using ModernSlavery.Hosts.Webjob.Jobs;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.Infrastructure.Messaging;
using ModernSlavery.Infrastructure.Storage;
using System.Net.Http;
using ModernSlavery.Infrastructure.Database.Classes;
using ModernSlavery.Infrastructure.Database;

namespace ModernSlavery.Hosts.Webjob
{
    public class DependencyModule : IDependencyModule
    {
        private readonly ILogger _logger;
        private readonly SharedOptions _sharedOptions;
        private readonly GovNotifyOptions _govNotifyOptions;

        public DependencyModule(
            ILogger<DependencyModule> logger, 
            SharedOptions sharedOptions,
            GovNotifyOptions govNotifyOptions)
        {
            _logger = logger;
            _sharedOptions = sharedOptions;
            _govNotifyOptions = govNotifyOptions;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient<GovNotifyEmailProvider>(nameof(GovNotifyEmailProvider),
                httpClient =>
                {
                    GovNotifyEmailProvider.SetupHttpClient(httpClient, _govNotifyOptions.ApiServer);
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(GovNotifyEmailProvider.GetRetryPolicy());

            services.AddSingleton<IJobActivator, AutofacJobActivator>();

            //Register the AutoMapper configurations in all domain assemblies
            services.AddAutoMapper(_sharedOptions.IsDevelopment());

            //Add the custom webjob name resolver
            services.AddSingleton<INameResolver,WebjobNameResolver>();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<DatabaseContext>().As<IDbContext>();
            builder.RegisterType<SqlRepository>().As<IDataRepository>().InstancePerDependency();

            //Register the messaging dependencies
            builder.RegisterType<Messenger>().As<IMessenger>().SingleInstance();
            builder.RegisterType<GovNotifyAPI>().As<IGovNotifyAPI>().SingleInstance();

            // Register the email template dependencies
            builder.RegisterInstance(new EmailTemplateRepository(FileSystem.ExpandLocalPath("~/App_Data/EmailTemplates")))
                .As<IEmailTemplateRepository>().SingleInstance();

            //Register some singletons
            builder.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance()
                .WithParameter("seed", _sharedOptions.ObfuscationSeed);

            // Register email provider dependencies
            builder.RegisterType<GovNotifyEmailProvider>()
                .SingleInstance()
                .WithAttributeFiltering()
                .WithParameter(
                    (p, ctx) => p.ParameterType == typeof(HttpClient),
                    (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient(nameof(GovNotifyEmailProvider)));

            builder.RegisterType<SmtpEmailProvider>().SingleInstance().WithAttributeFiltering();
            builder.RegisterType<EmailProvider>().SingleInstance().WithAttributeFiltering();

            // Need to register webJob class in Autofac as well
            builder.RegisterType<Functions>().InstancePerDependency();
            builder.RegisterType<DisableWebjobProvider>().SingleInstance();

        }


        public void Configure(ILifetimeScope lifetimeScope)
        {  
            var applicationLifetime = lifetimeScope.Resolve<IHostApplicationLifetime>(); 
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

            applicationLifetime.ApplicationStarted.Register(
                () =>
                {
                    // Summary:
                    //     Triggered when the application host has fully started and is about to wait for
                    //     a graceful shutdown.
                    _logger.LogInformation("Webjobs Application Started");
                });
            applicationLifetime.ApplicationStopping.Register(
                () =>
                {
                    // Summary:
                    //     Triggered when the application host is performing a graceful shutdown. Requests
                    //     may still be in flight. Shutdown will block until this event completes.
                    _logger.LogInformation("Webjobs Application Stopping");
                });
        }

        public void RegisterModules(IList<Type> modules)
        {
            //Register references dependency modules
            modules.AddDependency<ModernSlavery.BusinessDomain.Account.DependencyModule>();
            modules.AddDependency<ModernSlavery.BusinessDomain.Registration.DependencyModule>();
            modules.AddDependency<ModernSlavery.BusinessDomain.Submission.DependencyModule>();
            modules.AddDependency<ModernSlavery.BusinessDomain.Shared.DependencyModule>();
            modules.AddDependency<ModernSlavery.BusinessDomain.Viewing.DependencyModule>();

            //Register the file storage dependencies
            modules.AddDependency<FileStorageDependencyModule>();

            //Register the queue storage dependencies
            modules.AddDependency<QueueStorageDependencyModule>();

            //Register the log storage dependencies
            modules.AddDependency<Infrastructure.Logging.DependencyModule>();

            //Register the search dependencies
            modules.AddDependency<Infrastructure.Search.DependencyModule>();
            
            //Register the Companies House dependencies
            modules.AddDependency<Infrastructure.CompaniesHouse.DependencyModule>();
        }

        public class AutofacJobActivator : IJobActivator
        {
            private readonly IServiceProvider _serviceProvider;

            public AutofacJobActivator(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public T CreateInstance<T>()
            {
                return _serviceProvider.GetService<T>();
            }
        }
    }
}