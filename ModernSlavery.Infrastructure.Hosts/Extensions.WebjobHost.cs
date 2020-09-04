using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Interfaces;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using ModernSlavery.Infrastructure.Logging;

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
    }
}