using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Configuration;
using System.Collections.Generic;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Infrastructure.Logging;
using System.IO;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Infrastructure.Hosts
{
    public static class WebHost
    {
        public static IHostBuilder ConfigureWebHostBuilder<TStartupModule>(string applicationName = null, Dictionary<string, string> additionalSettings = null, params string[] commandlineArgs) where TStartupModule : class, IDependencyModule
        {
            var hostBuilder = Host.CreateDefaultBuilder(commandlineArgs);

            //Load the configuration
            var configBuilder = new ConfigBuilder(additionalSettings, commandlineArgs);
            var appConfig = configBuilder.Build();

            //Set the content root from the confif or environment
            var contentRoot = appConfig[HostDefaults.ContentRootKey];
            if (string.IsNullOrWhiteSpace(contentRoot))contentRoot = Directory.GetCurrentDirectory();
            if (!Directory.Exists(contentRoot)) throw new DirectoryNotFoundException($"Cannot find content root '{contentRoot}'");

            hostBuilder.UseContentRoot(contentRoot);

            hostBuilder.ConfigureAppConfiguration((hostBuilderContext, appConfigBuilder) =>
            {
                appConfigBuilder.AddConfiguration(appConfig);
            });

            //Load the configuration options
            var optionsBinder = new OptionsBinder(appConfig);
            var configOptions = optionsBinder.BindAssemblies();

            hostBuilder.ConfigureServices(optionsBinder.RegisterOptions);

            //Load all the dependency actions
            var dependencyBuilder = new DependencyBuilder();
            dependencyBuilder.Build<TStartupModule>(configOptions);

            //Register the callback to add dependent services - this is required here so IWebHostEnvironment is available to services
            hostBuilder.ConfigureServices(dependencyBuilder.RegisterDependencyServices);

            //Create the callback to register autofac dependencies only
            hostBuilder.ConfigureContainer<ContainerBuilder>(dependencyBuilder.RegisterDependencyServices);

            hostBuilder.ConfigureLogging((hostBuilderContext,loggingBuilder) =>
            {
                //Setup the seri logger
                hostBuilderContext.Configuration.SetupSerilogLogger();

                loggingBuilder.AddAzureQueueLogger(); //Use the custom logger
                loggingBuilder.AddApplicationInsights(); //log to app insights
                loggingBuilder.AddAzureWebAppDiagnostics(); //Log to live azure stream (honors the settings in the App Service logs section of the App Service page of the Azure portal)
            });

            //Register Autofac as the service provider
            hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            //Configure the host defaults
            hostBuilder.ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder.UseConfiguration(appConfig);

                //When enabled (or when the Environment is set to Development), the app captures detailed exceptions.
                if (appConfig.IsDevelopment())webHostBuilder.CaptureStartupErrors(true).UseSetting(WebHostDefaults.DetailedErrorsKey, "true");

                //Configure the kestrel server
                webHostBuilder.ConfigureKestrel((context, options) =>
                    {
                        options.AddServerHeader = false;
                        //options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
                    });

                //Specify the root path of the site
                var webRoot = appConfig[WebHostDefaults.WebRootKey];
                if (string.IsNullOrWhiteSpace(webRoot)) webRoot="wwwroot";
                var fullWebroot = Path.Combine(contentRoot, webRoot);
                if (!Directory.Exists(fullWebroot)) throw new DirectoryNotFoundException($"Cannot find web root '{fullWebroot}'");

                webHostBuilder.UseWebRoot(webRoot);

                //Register the callback to configure the web application
                webHostBuilder.Configure(dependencyBuilder.ConfigureHost);
            });

            return hostBuilder;
        }
    }
}