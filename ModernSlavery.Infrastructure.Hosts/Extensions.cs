using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Infrastructure.Configuration;
using ModernSlavery.Infrastructure.Logging;
using Serilog;
using Serilog.Core;

namespace ModernSlavery.Infrastructure.Hosts
{
    public static partial class Extensions
    {
        private static string _EnvironmentName;

        public static string EnvironmentName
        {
            get
            {
                if (_EnvironmentName == null)
                {
                    _EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    if (string.IsNullOrWhiteSpace(_EnvironmentName))
                        _EnvironmentName = Environment.GetEnvironmentVariable("ASPNET_ENV");
                    if (string.IsNullOrWhiteSpace(_EnvironmentName))
                        _EnvironmentName = Environment.GetEnvironmentVariable("Hosting:Environment");
                    if (string.IsNullOrWhiteSpace(_EnvironmentName))
                        _EnvironmentName = Environment.GetEnvironmentVariable("AzureWebJobsEnv");
                    if (string.IsNullOrWhiteSpace(_EnvironmentName))
                        _EnvironmentName =
                            Environment.GetEnvironmentVariable("Environment"); //This is used by webjobs SDK v3 
                    if (string.IsNullOrWhiteSpace(_EnvironmentName) &&
                        Environment.GetEnvironmentVariable("DEV_ENVIRONMENT").ToBoolean()) _EnvironmentName = "Local";
                    if (string.IsNullOrWhiteSpace(_EnvironmentName)) _EnvironmentName = "Local";
                }

                return _EnvironmentName;
            }
            set => _EnvironmentName = value;
        }

        public static DependencyBuilder ConfigureHost<TStartupModule>(this IHostBuilder hostBuilder, string applicationName=null, string contentRoot = null, Dictionary<string, string> additionalSettings = null, params string[] commandlineArgs) where TStartupModule : class, IDependencyModule
        {
            //Set the console title to the application name
            if (string.IsNullOrWhiteSpace(applicationName)) applicationName = typeof(TStartupModule).Assembly.GetName().Name;
            Console.Title = applicationName;

            //Add a handler for unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //Configure the host defaults
            hostBuilder.UseEnvironment(EnvironmentName);
            
            //Build the host configuration
            hostBuilder.ConfigureHostConfiguration(builder =>
                {
                    builder.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        [HostDefaults.ApplicationKey] = applicationName,
                        [HostDefaults.ContentRootKey] = string.IsNullOrWhiteSpace(contentRoot) ? AppContext.BaseDirectory : contentRoot,
                    });

                    if (commandlineArgs!=null && commandlineArgs.Any())builder.AddCommandLine(commandlineArgs);
                });

            //Build the app configuration
            
            hostBuilder.ConfigureAppConfiguration((ctx, configurationBuilder) =>
                {
                    var configBuilder = new ConfigBuilder(configurationBuilder);
                    var config = configBuilder.Build(ctx.HostingEnvironment.EnvironmentName, additionalSettings);

                    //Setup Threads
                    config.SetupThreads();

                    //Setup SeriLogger
                    config.SetupSerilogLogger();
                });


            //Configure the service dependencies
            var dependencyBuilder = new DependencyBuilder(nameof(ModernSlavery));

            hostBuilder.ConfigureServices((ctx, services) =>
            {
                //services.AddAutofac();

                //Load all the IOptions in the domain
                var optionsBinder = new OptionsBinder(services, ctx.Configuration, nameof(ModernSlavery));
                optionsBinder.BindAssemblies();

                //Register the startup module service dependencies and descendents
                dependencyBuilder.AddServices(services);
            });

            //Register Autofac as the service provider
            //hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            //Add the logging to the web host
            hostBuilder.ConfigureLogging(
                builder =>
                {
                    //For more info see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.2
                    builder.ClearProviders();
                    builder.AddConfiguration();
                    builder.AddDebug();
                    builder.AddConsole(); //Use the console
                    builder.AddEventSourceLogger(); //Log to windows event log
                    builder.AddAzureQueueLogger(); //Use the custom logger
                    builder.AddApplicationInsights(); //log to app insights
                    builder.AddAzureWebAppDiagnostics(); //Log to live azure stream (honors the settings in the App Service logs section of the App Service page of the Azure portal)
                });

            //hostBuilder.UseConsoleLifetime();
            return dependencyBuilder;
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
            var ioMin = config.GetValueOrDefault("MinIOThreads", 300);
            var workerMin = config.GetValueOrDefault("MinWorkerThreads", 300);
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