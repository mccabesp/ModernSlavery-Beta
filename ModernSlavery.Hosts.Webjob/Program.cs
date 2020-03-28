using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Hosts.Webjob.Jobs;
using ModernSlavery.Infrastructure.Hosts;
using Extensions = ModernSlavery.Infrastructure.Hosts.Extensions;

namespace ModernSlavery.Hosts.Webjob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    public class Program
    {
        private static async Task Main(string[] args)
        {
            //Create the web host
            var host = Host.CreateDefaultBuilder(args).ConfigureWebjobHostBuilder<DependencyModule>().Build();

            //Show thread availability
            Console.WriteLine(Extensions.GetThreadCount());

            //NOTE: Leave this here to ensure function dependencies resolve on startup rather than when each function method is invoked
            //      It is also also useful when debugging individual jobs locally
            var functions = host.Services.GetService(typeof(Functions));

            //Run the host
            await host.RunAsync().ConfigureAwait(false);
        }
    }

}