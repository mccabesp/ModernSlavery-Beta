using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Infrastructure.Hosts.WebHost;

namespace ModernSlavery.IdServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.Title = "ModernSlavery.IdentityServer4";

            //Add a handler for unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //Create the web host
            var hostBuilder = Host.CreateDefaultBuilder(args).ConfigureWebHostBuilder<Startup>();
            var host = hostBuilder.Build();

            //Show thread availability
            Console.WriteLine(Infrastructure.Hosts.Extensions.GetThreadCount());

            //Run the host
            await host.RunAsync();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            Console.WriteLine($"UNHANDLED EXCEPTION ({Console.Title}): {ex.Message}{Environment.NewLine}{ex.GetDetailsText()}");
            Debug.WriteLine($"UNHANDLED EXCEPTION ({Console.Title}): {ex.Message}{Environment.NewLine}{ex.GetDetailsText()}");

            //Show thread availability
            Console.WriteLine(Infrastructure.Hosts.Extensions.GetThreadCount());

            throw ex;
        }

    }
}
