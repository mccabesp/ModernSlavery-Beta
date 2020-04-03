using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Configuration;
using ModernSlavery.Infrastructure.Logging;

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

        public static IHostBuilder ConfigureHost<TStartupModule>(this IHostBuilder hostBuilder, string applicationName = null, string contentRoot = null, bool autoConfigureOnBuild=false,Dictionary<string, string> additionalSettings = null, params string[] commandlineArgs) where TStartupModule : class, IDependencyModule
        {
            //Set the console title to the application name
            if (string.IsNullOrWhiteSpace(applicationName)) applicationName = Assembly.GetEntryAssembly().GetName().Name;
            Console.Title = applicationName;

            //Add a handler for unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //Configure the host defaults
            hostBuilder.UseEnvironment(EnvironmentName);

            //Build the host configuration
            hostBuilder.ConfigureHostConfiguration(configBuilder =>
                {
                    configBuilder.AddEnvironmentVariables(prefix: "DOTNET_");
                    if (additionalSettings==null) additionalSettings=new Dictionary<string, string>();
                    additionalSettings[HostDefaults.ApplicationKey] = applicationName;
                    additionalSettings[HostDefaults.ContentRootKey] = string.IsNullOrWhiteSpace(contentRoot)
                        ? AppContext.BaseDirectory
                        : contentRoot;

                    configBuilder.AddInMemoryCollection(additionalSettings);
                    if (commandlineArgs != null && commandlineArgs.Any()) configBuilder.AddCommandLine(commandlineArgs);
                });

            //Configure the application configuration
            hostBuilder.ConfigureAppConfiguration((context, builder) =>
            {
                //Build the configuration and save till later
                var config = new ConfigBuilder(builder, EnvironmentName, additionalSettings).Build();

                //Setup Threads
                config.SetupThreads();
            });

            //Register the dependencies
            DependencyBuilder dependencyBuilder = null;
            hostBuilder.ConfigureServices((context, serviceCollection) =>
            {
                dependencyBuilder = new DependencyBuilder(context.Configuration, autoConfigureOnBuild: autoConfigureOnBuild);
                //Add any registered services to the dependency module builder which may be used by the options builder.
                dependencyBuilder.Services = serviceCollection;
            });
            
            
            //Register Autofac as the service provider
            var serviceProviderFactory = new AutofacServiceProviderFactory();
            hostBuilder.UseServiceProviderFactory(serviceProviderFactory);

            hostBuilder.ConfigureContainer<ContainerBuilder>((context, builder) =>
            {
                //Build all the required dependencies
                dependencyBuilder.Build<TStartupModule>(builder);
                dependencyBuilder.ServiceProviderFactory = serviceProviderFactory;
                hostBuilder.Properties.Add("serviceProviderFactory", serviceProviderFactory);
                hostBuilder.Properties.Add("dependencyBuilder", dependencyBuilder);
            });

            //Add the logging to the web host
            hostBuilder.ConfigureLogging(
                (hostingContext, logging) =>
                {
                    //Setup the seri logger
                    hostingContext.Configuration.SetupSerilogLogger();
                    
                    var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                    // IMPORTANT: This needs to be added *before* configuration is loaded, this lets
                    // the defaults be overridden by the configuration.
                    if (isWindows)
                    {
                        // Default the EventLogLoggerProvider to warning or above
                        logging.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Warning);
                    }

                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventSourceLogger();

                    // Add the EventLogLoggerProvider on windows machines
                    if (isWindows) logging.AddEventLog();

                    logging.AddAzureQueueLogger(); //Use the custom logger
                    logging.AddApplicationInsights(); //log to app insights
                    logging.AddAzureWebAppDiagnostics(); //Log to live azure stream (honors the settings in the App Service logs section of the App Service page of the Azure portal)
                });

            hostBuilder.UseConsoleLifetime();
            return hostBuilder;
        }

        /// <summary>
        /// Adds all the controller, taghelpers, views and razor pages from the assembly of the class
        /// Also, can turn on runtime compilation of razor views during development.
        ///
        /// This avoids having to add an explicit reference in the host library to a component in the Razor class library.
        /// Without this the razor views will not be found.
        /// </summary>
        /// <typeparam name="TModule">The entry class within the Razor Class library</typeparam>
        /// <param name="mvcBuilder">The mvcBuilder</param>
        /// <returns></returns>
        public static IMvcBuilder AddRazorClassLibrary<TModule>(this IMvcBuilder mvcBuilder) where TModule : class
        {
            var partAssembly = typeof(TModule).Assembly;

            mvcBuilder.AddApplicationPart(partAssembly);

            var razorAssemblyPath = Path.GetDirectoryName(partAssembly.Location);
            var razorAssemblyLocation = Path.Combine(razorAssemblyPath,$"{Path.GetFileNameWithoutExtension(partAssembly.Location)}.Views{Path.GetExtension(partAssembly.Location)}");
            var razorAssembly = Assembly.LoadFile(razorAssemblyLocation);
            mvcBuilder.PartManager.ApplicationParts.Add(new CompiledRazorAssemblyPart(razorAssembly));


            return mvcBuilder;
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

        public static void AddAutoMapper(this IServiceCollection services, bool assertConfigurationIsValid=false)
        {
            var assemblyPrefix = nameof(ModernSlavery);

            // Initialise AutoMapper
            var mapperConfig = new MapperConfiguration(config =>
            {
                AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.GetName().Name.StartsWith(assemblyPrefix, true, default)).ForEach(
                        assembly =>
                        {
                            // register all out mapper profiles (classes/mappers/*)
                            config.AddMaps(assembly);
                            // allows auto mapper to inject our dependencies
                            //config.ConstructServicesUsing(serviceTypeToConstruct =>
                            //{
                            //    //TODO
                            //});                    });
                        });
            });

            // only during development, validate your mappings; remove it before release
            if (assertConfigurationIsValid)mapperConfig.AssertConfigurationIsValid();


            //Add a single mapper to the dependency container
            services.AddSingleton(mapperConfig.CreateMapper());
        }

    }
}