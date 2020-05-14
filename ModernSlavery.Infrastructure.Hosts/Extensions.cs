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
using Microsoft.AspNetCore.Hosting;
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
        public static ConfigBuilder ConfigureHost(this IHostBuilder hostBuilder, string applicationName = null, string contentRoot = null, Dictionary<string, string> additionalSettings = null, params string[] commandlineArgs) 
        {
            if (string.IsNullOrWhiteSpace(applicationName)) applicationName = Assembly.GetEntryAssembly().GetName().Name;

            //Set the console title to the application name
            Console.Title = applicationName;

            //Add a handler for unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //Build the configuration and save till later
            var configBuilder = new ConfigBuilder();
            if (!string.IsNullOrWhiteSpace(applicationName)) configBuilder.AddApplicationName(applicationName);
            if (!string.IsNullOrWhiteSpace(contentRoot)) configBuilder.AddContentRoot(contentRoot);
            configBuilder.AddSettings(additionalSettings);
            configBuilder.AddCommandLineArgs(commandlineArgs);

            //Build the configuration and save till later
            var config = configBuilder.Build();

            Encryption.SetDefaultEncryptionKey(config["DefaultEncryptionKey"]);
            Encryption.EncryptEmails = config.GetValueOrDefault("EncryptEmails", true);

            //Initialise the virtual date and time
            VirtualDateTime.Initialise(config["DateTimeOffset"]);

            //Setup Threads
            config.SetupThreads();

            //Build the host configuration
            configBuilder.ConfigureHost(hostBuilder);

            //Register Autofac as the service provider
            var serviceProviderFactory = new AutofacServiceProviderFactory();
            hostBuilder.UseServiceProviderFactory(serviceProviderFactory);

            //Load all the IOptions in the domain
            var optionsBinder = new OptionsBinder(config);
            optionsBinder.BindAssemblies();

            hostBuilder.ConfigureServices((hostBuilderContext, serviceCollection) =>
            {
                //Add the configuration
                serviceCollection.AddSingleton(config);

                //Add the configuration options
                optionsBinder.Services.ForEach(service => serviceCollection.Add(service));
            });

            hostBuilder.UseConsoleLifetime();

            //Add the logging to the web host
            hostBuilder.ConfigureLogging(
                (hostBuilderContext, loggingBuilder) =>
                {
                    //Setup the seri logger
                    hostBuilderContext.Configuration.SetupSerilogLogger();

                    var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                    // IMPORTANT: This needs to be added *before* configuration is loaded, this lets
                    // the defaults be overridden by the configuration.
                    if (isWindows)
                    {
                        // Default the EventLogLoggerProvider to warning or above
                        loggingBuilder.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Warning);
                    }

                    loggingBuilder.AddConfiguration(hostBuilderContext.Configuration.GetSection("Logging"));
                    loggingBuilder.AddConsole();
                    loggingBuilder.AddDebug();
                    loggingBuilder.AddEventSourceLogger();

                    // Add the EventLogLoggerProvider on windows machines
                    if (isWindows) loggingBuilder.AddEventLog();

                    loggingBuilder.AddAzureQueueLogger(); //Use the custom logger
                    loggingBuilder.AddApplicationInsights(); //log to app insights
                    loggingBuilder.AddAzureWebAppDiagnostics(); //Log to live azure stream (honors the settings in the App Service logs section of the App Service page of the Azure portal)
                });

            return configBuilder;
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
            return mvcBuilder;
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