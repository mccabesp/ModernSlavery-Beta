using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Configuration;

namespace ModernSlavery.Infrastructure.Hosts
{
    public static class WebjobHost
    {
        public class WebJobsStartup: IWebJobsStartup
        {
            public void Configure(IWebJobsBuilder builder)
            {
                throw new System.NotImplementedException();
            }
        }
        public static IHostBuilder ConfigureWebjobHostBuilder<TStartupModule>(string applicationName=null, string contentRoot = null, string webRoot = null, params string[] commandlineArgs) where TStartupModule: class, IDependencyModule
        {
            var hostBuilder = new HostBuilder();

            //Setup the configuration sources and builder
            var dependencyBuilder = hostBuilder.ConfigureHost<TStartupModule>(applicationName, contentRoot, commandlineArgs: commandlineArgs);

            //Register the callback to add dependent services
            dependencyBuilder.PopulateHostServices(hostBuilder);

            hostBuilder.ConfigureWebJobs(webjobHostBuilder =>
                {
                    webjobHostBuilder.AddAzureStorageCoreServices();
                    webjobHostBuilder.AddAzureStorage(
                        queueConfig =>
                        {
                            queueConfig.BatchSize = 1; //Process queue messages 1 item per time per job function
                        },
                        blobConfig =>
                        {
                            //Configure blobs here
                        });
                    webjobHostBuilder.AddServiceBus();
                    webjobHostBuilder.AddEventHubs();
                    webjobHostBuilder.AddTimers();


                });

            return hostBuilder;
        }
    }
}