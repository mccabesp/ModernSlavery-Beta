using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using ModernSlavery.Infrastructure.Logging;
using System.Threading;

namespace ModernSlavery.Infrastructure.Hosts
{
    public static partial class Extensions
    {
        public static (IHostBuilder HostBuilder, DependencyBuilder DependencyBuilder, IConfiguration AppConfig) CreateGenericHost<TStartupModule>(string applicationName = null, Dictionary<string, string> additionalSettings = null, params string[] commandlineArgs) where TStartupModule : class, IDependencyModule
        {
            var hostBuilder = Host.CreateDefaultBuilder(commandlineArgs);

            //Load the configuration
            var configBuilder = new ConfigBuilder(additionalSettings, commandlineArgs);
            var appConfig = configBuilder.Build();

            //Set the content root to the bin folder if in development mode
            if (appConfig.IsDevelopment()) appConfig[HostDefaults.ContentRootKey] = System.AppDomain.CurrentDomain.BaseDirectory;

            //Set the content root from the confif or environment
            var contentRoot = appConfig[HostDefaults.ContentRootKey];

            if (string.IsNullOrWhiteSpace(contentRoot)) contentRoot = (appConfig[HostDefaults.ContentRootKey]=System.AppDomain.CurrentDomain.BaseDirectory);
            if (!Directory.Exists(contentRoot)) throw new DirectoryNotFoundException($"Cannot find content root '{contentRoot}'");

            hostBuilder.UseContentRoot(contentRoot);

            hostBuilder.ConfigureAppConfiguration((hostBuilderContext, appConfigBuilder) =>
            {
                appConfigBuilder.AddConfiguration(appConfig);
            });

            //Load the configuration options
            var optionsBinder = new OptionsBinder(appConfig);
            var configOptions = optionsBinder.BindAssemblies();

            hostBuilder.ConfigureServices(optionsBinder.RegisterOptions);

            //Load all the dependency actions
            var dependencyBuilder = new DependencyBuilder();
            dependencyBuilder.Build<TStartupModule>(configOptions);

            //Register the callback to add dependent services - this is required here so IWebHostEnvironment is available to services
            hostBuilder.ConfigureServices(dependencyBuilder.RegisterDependencyServices);

            //Create the callback to register autofac dependencies only
            hostBuilder.ConfigureContainer<ContainerBuilder>(dependencyBuilder.RegisterDependencyServices);

            hostBuilder.ConfigureLogging((hostBuilderContext, loggingBuilder) =>
            {
                //Setup the seri logger
                hostBuilderContext.Configuration.SetupSerilogLogger();

                loggingBuilder.AddAzureQueueLogger(); //Use the custom logger
                loggingBuilder.AddApplicationInsights(); //log to app insights
                loggingBuilder.AddAzureWebAppDiagnostics(); //Log to live azure stream (honors the settings in the App Service logs section of the App Service page of the Azure portal)
            });

            //Register Autofac as the service provider
            hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            return (hostBuilder, dependencyBuilder, appConfig);
        }

        public static IEnumerable<string> GetKeys(this IConfiguration config)
        {
            return config.GetChildren().Select(c => c.Key);
        }

        public static IEnumerable<string> GetHostAddresses(this IHost host)
        {
            var kestrelServer = host.Services.GetRequiredService<IServer>();
            return kestrelServer.Features.GetHostAddresses();
        }

        public static IEnumerable<string> GetHostAddresses(this IFeatureCollection features)
        {
            var addressFeature = features.Get<IServerAddressesFeature>();
            foreach (var address in addressFeature.Addresses)
            {
                yield return address.ReplaceI("127.0.0.1:", "localhost:");
            }
        }

        public static void LogHostAddresses(this IFeatureCollection features, ILogger logger)
        {
            foreach (var address in features.GetHostAddresses())
            {
                logger.LogInformation("Listening on: " + address);
            }
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

            var errorMessage = $"UNHANDLED EXCEPTION ({Console.Title}): {ex.Message}{Environment.NewLine}{ex.GetDetailsText()}";

            Console.WriteLine(errorMessage);
            if (Debugger.IsAttached)Debug.WriteLine(errorMessage);

            throw ex;
        }

        public static string GetThreadCount()
        {
            ThreadPool.GetMinThreads(out var workerMin, out var ioMin);
            ThreadPool.GetMaxThreads(out var workerMax, out var ioMax);
            ThreadPool.GetAvailableThreads(out var workerFree, out var ioFree);
            return $"Threads (Worker busy:{workerMax - workerFree:N0} min:{workerMin:N0} max:{workerMax:N0}, I/O busy:{ioMax - ioFree:N0} min:{ioMin:N0} max:{ioMax:N0})";
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