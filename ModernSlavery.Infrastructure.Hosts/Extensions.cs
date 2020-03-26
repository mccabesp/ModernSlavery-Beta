using System;
using System.Globalization;
using System.Threading;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Infrastructure.Configuration;
using Serilog;
using Serilog.Core;

namespace ModernSlavery.Infrastructure.Hosts
{
    public static class Extensions
    {
        public static IConfiguration BuildConfiguration(IConfigurationBuilder configurationBuilder = null)
        {
            var configBuilder = new ConfigBuilder(configurationBuilder);
            var config = configBuilder.Build();
            Encryption.SetDefaultEncryptionKey(config["DefaultEncryptionKey"]);
            Console.WriteLine($"AzureStorageConnectionString: {config.GetConnectionString("AzureStorage")}");
            Console.WriteLine($"Authority: {config["ExternalHost"]}");
            return config;
        }

        public static string GetThreadCount()
        {
            ThreadPool.GetMinThreads(out var workerMin, out var ioMin);
            ThreadPool.GetMaxThreads(out var workerMax, out var ioMax);
            ThreadPool.GetAvailableThreads(out var workerFree, out var ioFree);
            return
                $"Threads (Worker busy:{workerMax - workerFree:N0} min:{workerMin:N0} max:{workerMax:N0}, I/O busy:{ioMax - ioFree:N0} min:{ioMin:N0} max:{ioMax:N0})";
        }

        public static void LoadOptions<T>(this IConfiguration section, IServiceCollection services)
            where T : class, new()
        {
            // Register the IOptions object
            services.Configure<T>(section);

            // Explicitly register the settings object by delegating to the IOptions object this allows resolution of Options class without IOptions dependency
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<T>>().Value);
        }

        public static void SetupThreads(this IConfiguration config)
        {
            //Culture is required so UK dates can be parsed correctly
            Thread.CurrentThread.CurrentCulture = new CultureInfo(config.GetValue("Culture", "en-GB"));
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            CultureInfo.DefaultThreadCurrentCulture = Thread.CurrentThread.CurrentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = Thread.CurrentThread.CurrentCulture;

            //Set the minumum threads 
            Console.WriteLine(config.SetThreadCount());

            //Show thread availability
            Console.WriteLine(GetThreadCount());
        }

        public static string SetThreadCount(this IConfiguration config)
        {
            var ioMin = config.GetValue("MinIOThreads", 300);
            var workerMin = config.GetValue("MinWorkerThreads", 300);
            ThreadPool.SetMinThreads(workerMin, ioMin);
            return $"Min Threads Set (Work:{workerMin:N0}, I/O: {ioMin:N0})";
        }

        public static void SetupSerilogLogger(this IConfiguration config)
        {
            Logger log;

            if (config[""].EqualsI("LOCAL"))
                log = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            else
                log = new LoggerConfiguration().WriteTo
                    .ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Traces)
                    .CreateLogger();

            Log.Logger = log;
            Log.Information("Serilog logger setup complete");
        }
    }
}