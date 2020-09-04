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
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Configuration.Json;

namespace ModernSlavery.Infrastructure.Hosts
{
    public static partial class Extensions
    {
        public static (IHostBuilder HostBuilder, DependencyBuilder DependencyBuilder, IConfiguration AppConfig) CreateGenericHost<TStartupModule>(Dictionary<string, string> additionalSettings = null, params string[] commandlineArgs) where TStartupModule : class, IDependencyModule
        {
            //Create an unhandled exception handler forthe app domain
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var hostBuilder = Host.CreateDefaultBuilder(commandlineArgs);
            
            //Load the configuration
            var configBuilder = new ConfigBuilder(additionalSettings, commandlineArgs);
            var appConfig = configBuilder.Build();

            hostBuilder.ConfigureHostConfiguration(hostConfigBuilder =>
            {
                hostConfigBuilder.Sources.Clear();
                hostConfigBuilder.AddConfiguration(appConfig);
            });

            //Set the virtual date and time
            if (!appConfig.IsProduction() && !string.IsNullOrWhiteSpace(appConfig["DateTimeOffset"]))
                VirtualDateTime.Initialise(appConfig["DateTimeOffset"]);

            //Setup the threads to use the current culture from config or en-GB by default
            appConfig.SetupThreadCulture();

            //Set the content root to the bin folder if in development mode
            if (appConfig.IsDevelopment() || appConfig.ContainsSecretFiles()) appConfig[HostDefaults.ContentRootKey] = System.AppDomain.CurrentDomain.BaseDirectory;

            //Set the content root from the confif or environment
            var contentRoot = appConfig[HostDefaults.ContentRootKey];

            if (string.IsNullOrWhiteSpace(contentRoot)) contentRoot = (appConfig[HostDefaults.ContentRootKey]=System.AppDomain.CurrentDomain.BaseDirectory);
            if (!Directory.Exists(contentRoot)) throw new DirectoryNotFoundException($"Cannot find content root '{contentRoot}'");

            hostBuilder.UseContentRoot(contentRoot);

            hostBuilder.ConfigureAppConfiguration((hostBuilderContext, appConfigBuilder) =>
            {
                //Remove the extra json appsettings added by system
                appConfigBuilder.RemoveConfigSources<JsonConfigurationSource>();
                appConfigBuilder.AddConfiguration(appConfig);
            });

            hostBuilder.ConfigureLogging((hostBuilderContext, loggingBuilder) =>
            {
                loggingBuilder.ClearProviders();
                //Setup the seri logger
                hostBuilderContext.Configuration.SetupSerilogLogger();
                //loggingBuilder.AddAzureQueueLogger(); //Use the custom logger
                var instrumentationKey = appConfig["ApplicationInsights:InstrumentationKey"];
                loggingBuilder.AddApplicationInsights(instrumentationKey); //log to app insights
                loggingBuilder.AddAzureWebAppDiagnostics(); //Log to live azure stream (honors the settings in the App Service logs section of the App Service page of the Azure portal)
                loggingBuilder.AddConsole();
                loggingBuilder.AddConfiguration(appConfig);
            });

            //Load the configuration options
            var optionsBinder = new OptionsBinder(appConfig);
            var optionServices = optionsBinder.BindAssemblies();

            hostBuilder.ConfigureServices(optionsBinder.RegisterOptions);

            //Load all the dependency actions
            var dependencyBuilder = new DependencyBuilder();
            dependencyBuilder.Build<TStartupModule>(optionServices, appConfig);

            //Register the callback to add dependent services - this is required here so IWebHostEnvironment is available to services
            hostBuilder.ConfigureServices(dependencyBuilder.RegisterDependencyServices);

            //Create the callback to register autofac dependencies only
            hostBuilder.ConfigureContainer<ContainerBuilder>(dependencyBuilder.RegisterDependencyServices);

            //Register Autofac as the service provider
            hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            return (hostBuilder, dependencyBuilder, appConfig);
        }

        public static IEnumerable<string> GetKeys(this IConfiguration config)
        {
            return config.GetChildren().Select(c => c.Key);
        }

        public static string GetHostAddress(this IHost host)
        {
            var kestrelServer = host.Services.GetRequiredService<IServer>();
            return kestrelServer.Features.GetHostAddresses().FirstOrDefault();
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
                yield return address.Replace("127.0.0.1","localhost");
            }
        }

        public static void LogHostAddresses(this IFeatureCollection features, ILogger logger)
        {
            foreach (var address in features.GetHostAddresses())
            {
                Console.WriteLine("Listening on: " + address);
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
        public static IMvcBuilder AddApplicationPart<TModule>(this IMvcBuilder mvcBuilder) where TModule : class
        {
            var partAssembly = typeof(TModule).Assembly;
            return mvcBuilder.AddApplicationPart(partAssembly);
        }

        public static IMvcBuilder AddApplicationPart(this IMvcBuilder mvcBuilder, Assembly partAssembly)
        {
            var partFactory = ApplicationPartFactory.GetApplicationPartFactory(partAssembly);
            foreach (var applicationPart in partFactory.GetApplicationParts(partAssembly))
                mvcBuilder.PartManager.ApplicationParts.Add(applicationPart);

            var relatedAssemblies = RelatedAssemblyAttribute.GetRelatedAssemblies(partAssembly, throwOnError: true);
            foreach (var relatedAssembly in relatedAssemblies)
                AddApplicationPart(mvcBuilder, relatedAssembly);

            return mvcBuilder;
        }

        public static void AddApplicationPartsRuntimeCompilation(this IMvcBuilder mvcBuilder)
        {
            mvcBuilder.AddRazorRuntimeCompilation(mvcBuilder.PartManager.ApplicationParts.OfType<CompiledRazorAssemblyPart>().Select(p=>$"..\\{p.Name.BeforeLast(".Views")}").ToArray());
        }

        public static void AddRazorRuntimeCompilation(this IMvcBuilder mvcBuilder, params string[] folders)
        {
            var refsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "refs");
            if (!Directory.Exists(refsDirectory)) throw new DirectoryNotFoundException($"Cannot find reference directory '{refsDirectory}'");
            if (!folders.Any()) throw new ArgumentNullException(nameof(folders));

            Parallel.For(0,folders.Length,i =>
            {
                if (!Path.IsPathRooted(folders[i])) folders[i] = Path.Combine(Environment.CurrentDirectory, folders[i]);
                if (!Directory.Exists(folders[i])) throw new DirectoryNotFoundException($"Cannot find source directory '{folders[i]}'");
            });

            var refFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll").ToHashSet();
            refFiles.AddRange(Directory.GetFiles(refsDirectory));

            mvcBuilder.AddRazorRuntimeCompilation(options =>
            {
                folders.ForEach(f=>options.FileProviders.Add(new PhysicalFileProvider(f)));
                foreach (var refFile in refFiles) options.AdditionalReferencePaths.Add(refFile);
            });
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            
            var errorMessage = $"{(e.IsTerminating ? "FATAL " : "")}UNHANDLED EXCEPTION ({Console.Title}): {ex.Message}{Environment.NewLine}{ex.GetDetailsText()}";

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

        public static void SetupThreadCulture(this IConfiguration config)
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
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.GetName().Name.StartsWith(assemblyPrefix, true, default));
                assemblies
                    .ForEach(
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

            //Compile the mapping now rather than at runtime
            mapperConfig.CompileMappings();

            // only during development, validate your mappings; remove it before release
            if (assertConfigurationIsValid)mapperConfig.AssertConfigurationIsValid();


            //Add a single mapper to the dependency container
            services.AddSingleton(mapperConfig.CreateMapper());
        }
    }
}