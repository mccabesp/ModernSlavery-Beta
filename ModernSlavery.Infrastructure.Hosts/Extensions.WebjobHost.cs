using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Interfaces;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Infrastructure.Telemetry.AppInsights;
using System.Linq;
using Microsoft.ApplicationInsights.Extensibility;
using ModernSlavery.Core.Options;
using ModernSlavery.Infrastructure.Telemetry.AppInsights.Suppressions;

namespace ModernSlavery.Infrastructure.Hosts
{
    public static class WebjobHost
    {
        public static IHostBuilder ConfigureWebjobHostBuilder<TStartupModule>(Dictionary<string, string> additionalSettings = null, params string[] commandlineArgs) where TStartupModule : class, IDependencyModule
        {
            var genericHost = Extensions.CreateGenericHost<TStartupModule>(additionalSettings, commandlineArgs);

            genericHost.HostBuilder.ConfigureLogging((hostBuilderContext, loggingBuilder) =>
            {
                var instrumentationKey = genericHost.AppConfig["ApplicationInsights:InstrumentationKey"];
                loggingBuilder.AddApplicationInsightsWebJobs(o => { o.InstrumentationKey = instrumentationKey;});
            });

            //Register the callback to configure the web jobs
            
            genericHost.HostBuilder.ConfigureWebJobs((hostBuilderContext,webJobsBuilder) =>
            {
                webJobsBuilder.AddAzureStorageCoreServices();
                webJobsBuilder.AddAzureStorage(
                    queueConfig =>
                    {
                        queueConfig.BatchSize = 1; //Process queue messages 1 item per time per job function
                },
                    blobConfig =>
                    {
                    //Configure blobs here
                });
                webJobsBuilder.AddServiceBus();
                webJobsBuilder.AddEventHubs();
                webJobsBuilder.AddTimers();


                var instrumentationKey = genericHost.AppConfig["ApplicationInsights:InstrumentationKey"];
                if (!string.IsNullOrWhiteSpace(instrumentationKey))
                {
                    //Set telemetry role and remove telemetry sent to Application Insights
                    AddApplicationInsightsWebJobsTelemetryProcessor<AppInsightsTelemetryProcessor>(webJobsBuilder.Services);
                }

                genericHost.DependencyBuilder.Container_OnBuild += (lifetimeScope) => genericHost.DependencyBuilder.ConfigureHost(lifetimeScope);
            },
            jobhostOptions=> { },
            (hostBuilderContext, configBuilder) => 
                {
                    //Remove the extra json appsettings added by system
                    configBuilder.ConfigurationBuilder.RemoveConfigSources<JsonConfigurationSource>();
                }
            );
            return genericHost.HostBuilder;
        }

        private static void AddApplicationInsightsWebJobsTelemetryProcessor<T>(IServiceCollection services)
        {
            var configDescriptor = services.SingleOrDefault(tc => tc.ServiceType == typeof(TelemetryConfiguration));
            if (configDescriptor?.ImplementationFactory == null) return;

            var implFactory = configDescriptor.ImplementationFactory;

            services.Remove(configDescriptor);
            services.AddSingleton(provider => {
                if (!(implFactory.Invoke(provider) is TelemetryConfiguration config)) return null;

                var storageOptions = provider.GetRequiredService<StorageOptions>();
                var applicationInsightsOptions = provider.GetRequiredService<ApplicationInsightsOptions>();
                var telemetrySuppressionOptions = provider.GetRequiredService<TelemetrySuppressionOptions>();
                config.TelemetryProcessorChainBuilder.Use(next => new AppInsightsTelemetryProcessor(next, storageOptions, applicationInsightsOptions, telemetrySuppressionOptions));
                config.TelemetryProcessorChainBuilder.Build();

                return config;
            });
        }
    }
}