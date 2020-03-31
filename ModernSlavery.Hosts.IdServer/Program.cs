using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Infrastructure.Hosts;
using Extensions = ModernSlavery.Infrastructure.Hosts.Extensions;

namespace ModernSlavery.Hosts.IdServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //Create the host
            var host = CreateHostBuilder(args).Build();

            //Show thread availability
            Console.WriteLine(Extensions.GetThreadCount());

            //Run the host
            await host.RunAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// If the app uses Entity Framework Core, don't change the name or signature of the CreateHostBuilder method. The Entity Framework Core tools expect to find a CreateHostBuilder method that configures the host without running the app. For more information, see Design-time DbContext Creation (https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dbcontext-creation).
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            //Create the web host
            return WebHost.ConfigureWebHostBuilder<DependencyModule>(commandlineArgs: args);
        }
    }
}