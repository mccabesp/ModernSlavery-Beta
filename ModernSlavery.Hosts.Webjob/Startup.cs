﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.EmailTemplates;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.Infrastructure.Configuration;
using ModernSlavery.Infrastructure.Messaging;

namespace ModernSlavery.Hosts.Webjob
{
    public class Startup:IStartup
    {
        private readonly IConfiguration _Config;
        private readonly ILogger _Logger;
        private IServiceProvider _ServiceProvider;

        public Startup(IConfiguration config)
        {
            _Config = config;
            _Logger = Activator.CreateInstance<Logger<Startup>>();
        }

        // ConfigureServices is where you register dependencies. This gets
        // called by the runtime before the ConfigureContainer method, below.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //Load all configuration options and use them to register all dependencies
            return services.SetupDependencies<DependencyModule>(_Config);
        }

        public void Configure(IApplicationBuilder app=null)
        {
            var fileRepository = _ServiceProvider.GetService<IFileRepository>();
            var sharedOptions = _ServiceProvider.GetService<SharedOptions>();

            //Initialise the virtual date and time
            VirtualDateTime.Initialise(sharedOptions.DateTimeOffset);

            //Set the default encryption key
            Encryption.SetDefaultEncryptionKey(sharedOptions.DefaultEncryptionKey);

            //Ensure SicSectorSynonyms exist on remote 


            //Initialise the virtual date and time
            VirtualDateTime.Initialise(sharedOptions.DateTimeOffset);

            Task.WaitAll(Core.Classes.Extensions.PushRemoteFileAsync(fileRepository, Filenames.SicSectorSynonyms, sharedOptions.DataPath));

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