using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Hosts.Webjob.Jobs;
using ModernSlavery.Infrastructure.Hosts.WebjobHost;
using Extensions = ModernSlavery.Infrastructure.Hosts.Extensions;

namespace ModernSlavery.Hosts.Webjob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    public class Program
    {
        public static IContainer ContainerIOC;

        private static async Task Main(string[] args)
        {
            Console.Title = "ModernSlavery.WebJobs";

            //Add a handler for unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //Create the web host
            var hostBuilder = Host.CreateDefaultBuilder(args).ConfigureWebjobHostBuilder<Startup>();
            var host = hostBuilder.Build();

            //Show thread availability
            Console.WriteLine(Extensions.GetThreadCount());

            //Leave this check here to ensure function dependencies resolve on startup rather than when each function method is invoked
            var functions = ContainerIOC.Resolve<Functions>();

            //Run the host
            await host.RunAsync();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            Console.WriteLine(
                $"UNHANDLED EXCEPTION ({Console.Title}): {ex.Message}{Environment.NewLine}{ex.GetDetailsText()}");
            Debug.WriteLine(
                $"UNHANDLED EXCEPTION ({Console.Title}): {ex.Message}{Environment.NewLine}{ex.GetDetailsText()}");

            //Show thread availability
            Console.WriteLine(Extensions.GetThreadCount());

            throw ex;
        }
    }

    public class AutofacJobActivator : IJobActivator
    {
        private readonly IContainer _container;

        public AutofacJobActivator(IContainer container)
        {
            _container = container;
        }

        public T CreateInstance<T>()
        {
            return _container.Resolve<T>();
        }
    }
}