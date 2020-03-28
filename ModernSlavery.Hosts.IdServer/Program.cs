using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Infrastructure.Hosts;
using Extensions = ModernSlavery.Infrastructure.Hosts.Extensions;

namespace ModernSlavery.IdServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //Create the web host
            var host = Host.CreateDefaultBuilder(args).ConfigureWebHostBuilder<DependencyModule>().Build();

            //Show thread availability
            Console.WriteLine(Extensions.GetThreadCount());

            //Run the host
            await host.RunAsync().ConfigureAwait(false);
        }
    }
}