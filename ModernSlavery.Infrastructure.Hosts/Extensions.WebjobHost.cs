using Autofac;
using Autofac.Core.Lifetime;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Configuration;
using System.Collections.Generic;

namespace ModernSlavery.Infrastructure.Hosts
{
    public static class WebjobHost
    {
        public static IHostBuilder ConfigureWebjobHostBuilder<TStartupModule>(Dictionary<string, string> additionalSettings = null, params string[] commandlineArgs) where TStartupModule : class, IDependencyModule
        {
            var genericHost = Extensions.CreateGenericHost<TStartupModule>(additionalSettings, commandlineArgs);
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