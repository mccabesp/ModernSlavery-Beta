using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Infrastructure.Logging;

namespace ModernSlavery.Infrastructure.Hosts
{
    public static partial class Extensions
    {
        public static IHostBuilder ConfigureWebjobHostBuilder<TStartupModule>(this IHostBuilder hostBuilder, string applicationName=null, string contentRoot = null, string webRoot = null) where TStartupModule : class, IDependencyModule
        {
            var dependencyBuilder=hostBuilder.ConfigureHost<TStartupModule>(applicationName, contentRoot);

            hostBuilder.ConfigureWebJobs(builder =>
                {
                    builder.AddAzureStorageCoreServices();
                    builder.AddAzureStorage(
                        queueConfig =>
                        {
                            queueConfig.BatchSize = 1; //Process queue messages 1 item per time per job function
                        },
                        blobConfig =>
                        {
                            //Configure blobs here
                        });
                    builder.AddServiceBus();
                    builder.AddEventHubs();
                    builder.AddTimers();
                    dependencyBuilder.ConfigureModules();
                });

            return hostBuilder;
        }
    }
}