using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Configuration;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using System;

namespace ModernSlavery.Infrastructure.Hosts
{
    public static class WebHost
    {
        public static IHostBuilder ConfigureWebHostBuilder<TStartupModule>(Dictionary<string, string> additionalSettings = null, params string[] commandlineArgs) where TStartupModule : class, IDependencyModule
        {
            var genericHost = Extensions.CreateGenericHost<TStartupModule>(additionalSettings, commandlineArgs);

            //Configure the host defaults
            genericHost.HostBuilder.ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder.UseConfiguration(genericHost.AppConfig);

                //When enabled (or when the Environment is set to Development), the app captures detailed exceptions.
                if (genericHost.AppConfig.IsDevelopment()) webHostBuilder.CaptureStartupErrors(true).UseSetting(WebHostDefaults.DetailedErrorsKey, "true");

                //Configure the kestrel server
                webHostBuilder.ConfigureKestrel((context, options) =>
                    {
                        options.AddServerHeader = false;
                        //options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
                    });

                //Specify the root path of the site
                var webRoot = genericHost.AppConfig[WebHostDefaults.WebRootKey];
                var contentRoot = genericHost.AppConfig[WebHostDefaults.ContentRootKey];
                if (string.IsNullOrWhiteSpace(webRoot)) webRoot = (genericHost.AppConfig[WebHostDefaults.WebRootKey]="wwwroot");
                var fullWebroot = Path.Combine(contentRoot, webRoot);
                if (!Directory.Exists(fullWebroot)) throw new DirectoryNotFoundException($"Cannot find web root '{fullWebroot}'");

                webHostBuilder.UseWebRoot(webRoot);

                //Register the callback to configure the web application
                webHostBuilder.Configure(appBuilder => {
                    ConfigureHost(genericHost.DependencyBuilder, appBuilder);
                });
            });

            return genericHost.HostBuilder;
        }

        private static void ConfigureHost(DependencyBuilder dependencyBuilder, IApplicationBuilder appBuilder)
        {
            var lifetimeScope = appBuilder.ApplicationServices.GetRequiredService<ILifetimeScope>();

            //Only add the appbuildder temporarily
            using var innerScope = lifetimeScope.BeginLifetimeScope(b => b.RegisterInstance(appBuilder).SingleInstance().ExternallyOwned());

            dependencyBuilder.ConfigureHost(innerScope);
        }

        public static void AddForwardedHeaders(this IServiceCollection services, params string[] allowedHosts)
        {
            if (allowedHosts == null || allowedHosts.Length == 0) throw new ArgumentNullException(nameof(allowedHosts));

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;

                options.KnownNetworks.Clear(); //its loopback by default
                options.KnownProxies.Clear();
                //options.ForwardedHostHeaderName = "X-Original-Host"; //This is for Application Gateway

                // Put your front door FQDN here and any other hosts that will send headers you want respected
                options.AllowedHosts = new List<string>(allowedHosts);
            });
        }

    }
}