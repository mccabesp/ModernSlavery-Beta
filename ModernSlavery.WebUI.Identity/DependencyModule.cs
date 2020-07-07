using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Identity.Classes;

namespace ModernSlavery.WebUI.Identity
{
    public class DependencyModule : IDependencyModule
    {
        private readonly ILogger _logger;
        private readonly IdentityServerOptions _identityServerOptions;
        private readonly SharedOptions _sharedOptions;

        public DependencyModule(
            ILogger<DependencyModule> logger,
            IdentityServerOptions identityServerOptions,
            SharedOptions sharedOptions)
        {
            _logger = logger;
            _sharedOptions = sharedOptions;
            _identityServerOptions = identityServerOptions;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            #region Configure Identity Server

            services.AddSingleton<IEventSink, AuditEventSink>();

            var identityServer = services.AddIdentityServer(
                    options =>
                    {
                        options.Events.RaiseSuccessEvents = true;
                        options.Events.RaiseFailureEvents = true;
                        options.Events.RaiseErrorEvents = true;
                        options.UserInteraction.LoginUrl = "/identity/sign-in";
                        options.UserInteraction.LogoutUrl = "/identity/sign-out";
                        options.UserInteraction.ErrorUrl = "/identity/error";
                    })
                .AddInMemoryClients(_identityServerOptions.Clients)
                .AddInMemoryIdentityResources(new List<IdentityResource>
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile(),
                    new IdentityResource {Name = "roles", UserClaims = new List<string> {ClaimTypes.Role}}
                })
                .AddResourceOwnerValidator<CustomResourceOwnerPasswordValidator>();
            //.AddProfileService<CustomProfileService>();

            //identityServer.Services.AddScoped<IUserRepository, UserRepository>();
            
            if (string.IsNullOrWhiteSpace(_sharedOptions.CertThumprint))
                identityServer.AddDeveloperSigningCredential();
            else
                identityServer.AddSigningCredential(LoadCertificate(_sharedOptions.CertThumprint, _sharedOptions.CertExpiresWarningDays));

            #endregion
        }


        public void ConfigureContainer(ContainerBuilder builder)
        {

        }


        public void Configure(ILifetimeScope lifetimeScope)
        {
            //Add configuration here
            var app = lifetimeScope.Resolve<IApplicationBuilder>();
            app.UseIdentityServer();
        }

        public void RegisterModules(IList<Type> modules)
        {

        }

        private X509Certificate2 LoadCertificate(string certThumprint, int certExpiresWarningDays)
        {
            if (string.IsNullOrWhiteSpace(certThumprint)) throw new ArgumentNullException(nameof(certThumprint));

            //Load the site certificate
            var cert = HttpsCertificate.LoadCertificateFromThumbprint(certThumprint);

            var expires = cert.GetExpirationDateString().ToDateTime();
            var remainingTime = expires - VirtualDateTime.Now;
            if (expires < VirtualDateTime.UtcNow)
                _logger.LogError($"Certificate '{cert.FriendlyName}' from thumbprint '{certThumprint}' expired on {expires.ToFriendlyDate()} and needs replacing immediately.");
            else if (expires < VirtualDateTime.UtcNow.AddDays(certExpiresWarningDays))
                _logger.LogWarning($"Certificate '{cert.FriendlyName}' from thumbprint '{certThumprint}' is due expire on {expires.ToFriendlyDate()} and will need replacing within {remainingTime.ToFriendly(maxParts: 2)}.");
            else
                _logger.LogInformation($"Successfully loaded certificate '{cert.FriendlyName}' from thumbprint '{certThumprint}' and expires '{cert.GetExpirationDateString()}'");

            return cert;
        }
    }
}