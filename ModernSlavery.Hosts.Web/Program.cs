using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.WebUI;

// ReSharper disable once CheckNamespace
namespace ModernSlavery.Hosts.Web
{
    public static class Program
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