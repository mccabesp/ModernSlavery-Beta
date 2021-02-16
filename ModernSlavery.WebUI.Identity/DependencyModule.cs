using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
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
                        if (!string.IsNullOrWhiteSpace(_identityServerOptions.PublicOrigin))options.PublicOrigin = _identityServerOptions.PublicOrigin;
                        options.Events.RaiseSuccessEvents = true;
                        options.Events.RaiseFailureEvents = true;
                        options.Events.RaiseErrorEvents = true;
                        options.UserInteraction.LoginUrl = "/identity/sign-in";
                        options.UserInteraction.LogoutUrl = "/identity/sign-out";
                        options.UserInteraction.ErrorUrl = "/identity/error";
                        options.Authentication.CookieLifetime= TimeSpan.FromMinutes(_sharedOptions.SessionTimeOutMinutes);
                        options.Authentication.CookieSlidingExpiration = true;
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

            if (!string.IsNullOrWhiteSpace(_sharedOptions.CertThumbprint))
                identityServer.AddSigningCredential(LoadCertificate(_sharedOptions.CertThumbprint, _sharedOptions.CertExpiresWarningDays));
            else if (!string.IsNullOrWhiteSpace(_sharedOptions.CertFilepath) && !string.IsNullOrWhiteSpace(_sharedOptions.CertPassword))
                identityServer.AddSigningCredential(LoadCertificate(_sharedOptions.CertFilepath, _sharedOptions.CertPassword, _sharedOptions.CertExpiresWarningDays));
            else
                identityServer.AddDeveloperSigningCredential();

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

            IdentityModelEventSource.ShowPII = _identityServerOptions.ShowPII || _sharedOptions.UseDeveloperExceptions;

        }

        public void RegisterModules(IList<Type> modules)
        {

        }

        private X509Certificate2 LoadCertificate(string certThumbprint, int certExpiresWarningDays)
        {
            if (string.IsNullOrWhiteSpace(certThumbprint)) throw new ArgumentNullException(nameof(certThumbprint));

            //Load the site certificate
            var cert = HttpsCertificate.LoadCertificateFromThumbprint(certThumbprint);

            var expires = cert.GetExpirationDateString().ToDateTime();
            var remainingTime = expires - VirtualDateTime.Now;
            if (expires < VirtualDateTime.UtcNow)
                _logger.LogError($"Certificate '{cert.FriendlyName}' from thumbprint '{certThumbprint}' expired on {expires.ToFriendlyDate()} and needs replacing immediately.");
            else if (expires < VirtualDateTime.UtcNow.AddDays(certExpiresWarningDays))
                _logger.LogWarning($"Certificate '{cert.FriendlyName}' from thumbprint '{certThumbprint}' is due expire on {expires.ToFriendlyDate()} and will need replacing within {remainingTime.ToFriendly(maxParts: 2)}.");
            else
                _logger.LogInformation($"Successfully loaded certificate '{cert.FriendlyName}' from thumbprint '{certThumbprint}' and expires '{cert.GetExpirationDateString()}'");

            return cert;
        }

        private X509Certificate2 LoadCertificate(string filepath, string password, int certExpiresWarningDays)
        {
            if (string.IsNullOrWhiteSpace(filepath)) throw new ArgumentNullException(nameof(filepath));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));

            //Load the site certificate
            var cert = HttpsCertificate.LoadCertificateFromFile(filepath, password);

            var expires = cert.GetExpirationDateString().ToDateTime();
            var remainingTime = expires - VirtualDateTime.Now;
            if (expires < VirtualDateTime.UtcNow)
                _logger.LogError($"Certificate '{cert.FriendlyName}' from file '{filepath}' expired on {expires.ToFriendlyDate()} and needs replacing immediately.");
            else if (expires < VirtualDateTime.UtcNow.AddDays(certExpiresWarningDays))
                _logger.LogWarning($"Certificate '{cert.FriendlyName}' from file '{filepath}' is due expire on {expires.ToFriendlyDate()} and will need replacing within {remainingTime.ToFriendly(maxParts: 2)}.");
            else
                _logger.LogInformation($"Successfully loaded certificate '{cert.FriendlyName}' from file '{filepath}' and expires '{cert.GetExpirationDateString()}'");

            return cert;
        }
    }
}