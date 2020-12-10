using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Interfaces;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights.Extensibility;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using ModernSlavery.Infrastructure.Telemetry;

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
                loggingBuilder.AddApplicationInsightsWebJobs(o => { o.InstrumentationKey = instrumentationKey; });
            });

            //Register the callback to configure the web jobs
            genericHost.HostBuilder.ConfigureWebJobs(webJobsBuilder =>
            {
                //Add filter to prevent logging of file not found 404 errors
                //webJobsBuilder.AddFileNotFoundTelemetryFilter();

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
                
                genericHost.DependencyBuilder.Container_OnBuild += (lifetimeScope) => genericHost.DependencyBuilder.ConfigureHost(lifetimeScope);
            });
            return genericHost.HostBuilder;
        }

        /// <summary>
        /// Add filter to removes Http 404 (NotFound) errors received from file storage from telemetry sent to Application Insights
        /// NOTE: This is a workaround since the Webjobs SDK does not allow adding of ITelemetryProcessor
        /// For more information see https://github.com/Azure/azure-functions-host/issues/3741
        /// </summary>
        public static void AddFileNotFoundTelemetryFilter(this IWebJobsBuilder builder)
        {
            var configDescriptor = builder.Services.SingleOrDefault(tc => tc.ServiceType == typeof(TelemetryConfiguration));
            if (configDescriptor?.ImplementationFactory != null)
            {
                var implFactory = configDescriptor.ImplementationFactory;
                builder.Services.Remove(configDescriptor);
                builder.Services.AddSingleton(provider =>
                {
                    if (implFactory.Invoke(provider) is TelemetryConfiguration config)
                    {
                        var newConfig = new TelemetryConfiguration(config.InstrumentationKey,config.TelemetryChannel);
                        newConfig.ApplicationIdProvider = config.ApplicationIdProvider;
                        newConfig.InstrumentationKey = config.InstrumentationKey;
                        newConfig.TelemetryProcessorChainBuilder.Use(next => new FileNotFoundTelemetryFilter(next));

                        foreach (var processor in config.TelemetryProcessors)
                        {
                            newConfig.TelemetryProcessorChainBuilder.Use(next => processor);
                        }

                        var quickPulseProcessor = config.TelemetryProcessors.OfType<QuickPulseTelemetryProcessor>().FirstOrDefault();
                        if (quickPulseProcessor != null)
                        {
                            var quickPulseModule = new QuickPulseTelemetryModule();
                            quickPulseModule.RegisterTelemetryProcessor(quickPulseProcessor);
                            newConfig.TelemetryProcessorChainBuilder.Use(next => quickPulseProcessor);
                        }
                        newConfig.TelemetryProcessorChainBuilder.Build();
                        newConfig.TelemetryProcessors.OfType<ITelemetryModule>().ToList().ForEach(module => module.Initialize(newConfig));
                        return newConfig;
                    }
                    return null;
                });
            }
        }
    }
}