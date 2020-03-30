using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Extensions;
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
        public static IHostBuilder ConfigureWebjobHostBuilder(string applicationName=null, string contentRoot = null, string webRoot = null, params string[] commandlineArgs)
        {
            var hostBuilder = new HostBuilder();
            
            hostBuilder.ConfigureHost(applicationName, contentRoot, autoConfigureOnBuild: true, commandlineArgs:commandlineArgs);
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