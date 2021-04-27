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
using ModernSlavery.Infrastructure.Database.Classes;
using ModernSlavery.Infrastructure.Database;
using ModernSlavery.Infrastructure.Messaging.GovNotify;
using ModernSlavery.Infrastructure.Telemetry.AppInsights;
using System.Reflection;
using ModernSlavery.Core.Classes.StatementTypeIndexes;

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
            services.AddSingleton<IJobActivator, AutofacJobActivator>();

            //Register the AutoMapper configurations in all domain assemblies
            services.AddAutoMapper(_sharedOptions.IsDevelopment());

            //Register sector types
            services.AddSingleton<SectorTypeIndex>();

            //Add the custom webjob name resolver
            services.AddSingleton<INameResolver,WebJobNameResolver>();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<DatabaseContext>().As<IDbContext>().InstancePerLifetimeScope();
            
            builder.RegisterType<SqlRepository>().As<IDataRepository>().InstancePerLifetimeScope();

            //Register the messaging dependencies
            builder.RegisterType<SmtpMessenger>().As<ISmtpMessenger>().SingleInstance();

            // Register the email template dependencies
            builder.RegisterInstance(new EmailTemplateRepository(FileSystem.ExpandLocalPath("~/App_Data/EmailTemplates"))).As<IEmailTemplateRepository>().SingleInstance();

            //Register some singletons
            builder.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance().WithParameter("seed", _sharedOptions.ObfuscationSeed);

            // Register email provider dependencies
            builder.RegisterType<GovNotifyEmailProvider>().SingleInstance().WithAttributeFiltering();

            builder.RegisterType<SmtpEmailProvider>().SingleInstance().WithAttributeFiltering();
            builder.RegisterType<EmailProvider>().SingleInstance().WithAttributeFiltering();

            //Register all the webjobs
            var assembly = Assembly.GetExecutingAssembly();
            builder.RegisterAssemblyTypes(assembly).Where(t => typeof(IWebJob).IsAssignableFrom(t)).InstancePerLifetimeScope().WithAttributeFiltering();

            // Need to register webJob class in Autofac as well
            builder.RegisterType<DisableWebJobProvider>().SingleInstance();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {  
            var applicationLifetime = lifetimeScope.Resolve<IHostApplicationLifetime>();

            var config = lifetimeScope.Resolve<IConfiguration>();
            var fileRepository = lifetimeScope.Resolve<IFileRepository>();
            var sharedOptions = lifetimeScope.Resolve<SharedOptions>();

            // Register email templates
            var emailTemplatesConfigPath = "Email:Templates";
            // MSU templates
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
            RegisterEmailTemplate<MsuOrganisationRegistrationRequestTemplate>(emailTemplatesConfigPath);
            RegisterEmailTemplate<SendPINTemplate>(emailTemplatesConfigPath);

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

            Task.WaitAll(fileRepository.PushRemoteFileAsync(Filenames.SicSectorSynonyms, sharedOptions.AppDataPath));

            applicationLifetime.ApplicationStarted.Register(
                () =>
                {
                    // Summary:
                    //     Triggered when the application host has fully started and is about to wait for
                    //     a graceful shutdown.
                    _logger.LogInformation("WebJobs Application Started");
                });
            applicationLifetime.ApplicationStopping.Register(
                () =>
                {
                    // Summary:
                    //     Triggered when the application host is performing a graceful shutdown. Requests
                    //     may still be in flight. Shutdown will block until this event completes.
                    _logger.LogInformation("WebJobs Application Stopping");
                });
        }

        public void RegisterModules(IList<Type> modules)
        {
            //Register references dependency modules
            modules.AddDependency<BusinessDomain.Account.DependencyModule>();
            modules.AddDependency<BusinessDomain.Registration.DependencyModule>();
            modules.AddDependency<BusinessDomain.Submission.DependencyModule>();
            modules.AddDependency<BusinessDomain.Shared.DependencyModule>();
            modules.AddDependency<BusinessDomain.Viewing.DependencyModule>();

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

            //Register the Gov Notify dependencies
            modules.AddDependency<Infrastructure.Messaging.GovNotify.DependencyModule>();
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
                //Always run the webjobs in their own lifetime scope
                if (typeof(IWebJob).IsAssignableFrom(typeof(T)))
                {
                    var scope = _serviceProvider.CreateScope();
                    var job = scope.ServiceProvider.GetRequiredService<T>();
                    if (job is WebJob webJob) webJob.ServiceScope = scope;
                    return job;
                }

                //Other dependancies will use application scope
                return _serviceProvider.GetService<T>();
            }
        }
    }
}