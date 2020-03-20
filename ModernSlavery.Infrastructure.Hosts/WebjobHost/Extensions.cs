using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModernSlavery.Infrastructure.Storage;

namespace ModernSlavery.Infrastructure.Hosts.WebjobHost
{
    public static class Extensions
    {
        public static IHostBuilder ConfigureWebjobHostBuilder<TStartup>(this IHostBuilder hostBuilder,
            string contentRoot = null, string webRoot = null) where TStartup : IStartup
        {
            //Build the configuration settings
            var config = Hosts.Extensions.BuildConfiguration();

            //Setup Threads
            config.SetupThreads();

            //Setup SeriLogger
            config.SetupSerilogLogger();

            //Configure the host defaults
            hostBuilder.UseEnvironment(config["Environment"]);

            //Load the services from the startup class
            var startup = (TStartup) Activator.CreateInstance(typeof(TStartup), config);
            hostBuilder.ConfigureServices((builder, services) => { startup.ConfigureServices(services); });

            //Add the logging to the web host
            hostBuilder.ConfigureLogging(
                builder =>
                {
                    //For more info see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.2
                    builder.ClearProviders();
                    builder.AddConfiguration(config.GetSection("Logging"));
                    builder.AddDebug();
                    builder.AddConsole();
                    builder.AddEventSourceLogger(); //Log to windows event log
                    builder.AddAzureQueueLogger();
                });

            hostBuilder.ConfigureWebJobs(
                builder =>
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
                });

            hostBuilder.UseConsoleLifetime();
            return hostBuilder;
        }
    }
}