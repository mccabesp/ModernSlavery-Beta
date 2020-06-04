using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Configuration;
using System.Collections.Generic;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Infrastructure.Logging;

namespace ModernSlavery.Infrastructure.Hosts
{
    public static class WebjobHost
    {
        public static IHostBuilder ConfigureWebjobHostBuilder<TStartupModule>(string applicationName=null, string contentRoot = null, string webRoot = null, Dictionary<string, string> additionalSettings = null, params string[] commandlineArgs) where TStartupModule: class, IDependencyModule
        {
            var hostBuilder = Host.CreateDefaultBuilder(commandlineArgs);
            if (!string.IsNullOrWhiteSpace(contentRoot)) hostBuilder.UseContentRoot(contentRoot);

            //Load the configuration
            var configBuilder = new ConfigBuilder(additionalSettings);
            var appConfig = configBuilder.Build();

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

            //Create the callback to register service dependencies
            hostBuilder.ConfigureServices(dependencyBuilder.RegisterDependencyServices);

            //Create the callback to register autofac dependencies and execute dependency config actions
            hostBuilder.ConfigureContainer<ContainerBuilder>(containerBuilder => dependencyBuilder.RegisterDependencyServices(containerBuilder, true));

            hostBuilder.ConfigureLogging((hostBuilderContext, loggingBuilder) =>
            {
                //Setup the seri logger
                hostBuilderContext.Configuration.SetupSerilogLogger();

                loggingBuilder.AddAzureQueueLogger(); //Use the custom logger
                loggingBuilder.AddApplicationInsights(); //log to app insights
                loggingBuilder.AddAzureWebAppDiagnostics(); //Log to live azure stream (honors the settings in the App Service logs section of the App Service page of the Azure portal)
            });

            //Register Autofac as the service provider
            hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());


            return hostBuilder;
        }
    }
}