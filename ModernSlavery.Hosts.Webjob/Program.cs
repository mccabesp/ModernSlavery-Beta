﻿using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModernSlavery.Hosts.Webjob.Jobs;
using ModernSlavery.Infrastructure.Hosts;

namespace ModernSlavery.Hosts.Webjob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    public class Program
    {
        private static async Task Main(string[] args)
        {
            //Create the host
            var host = CreateHostBuilder(args).Build();

            //NOTE: Leave this here to ensure function dependencies resolve on startup rather than when each function method is invoked
            //      It is also also useful when debugging individual jobs locally
            //var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            //var logger = loggerFactory.CreateLogger<ILogger<UpdateFromCompaniesHouseWebJob>>();
            //var webjob = host.Services.GetRequiredService<UpdateFromCompaniesHouseWebJob>();
            //webjob.UpdateFromCompaniesHouseAsync(null, logger).Wait();
            //return;

            //Clear old console logs to prevent server crash on startup
            host.Services.GetRequiredService<ClearConsoleLogsWebJob>()?.ClearConsoleLogs();

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
            //Create the WebJobHost
            return WebjobHost.ConfigureWebjobHostBuilder<DependencyModule>(commandlineArgs: args);
        }
    }

}