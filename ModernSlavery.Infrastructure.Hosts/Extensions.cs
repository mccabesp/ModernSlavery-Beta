using Autofac;
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
using System.Threading;

namespace ModernSlavery.Infrastructure.Hosts
{
    public static partial class Extensions
    {

        public static void AddConfigSources(this IConfigurationBuilder configBbuilder, IHostEnvironment env)
        {
            if (env == null) throw new ArgumentNullException(nameof(env));

            //Make sure we know the environment
            if (string.IsNullOrWhiteSpace(env.EnvironmentName)) throw new ArgumentNullException(nameof(env.EnvironmentName));

            var config = configBbuilder.Build();

            //Add the azure key vault to configuration
            var vault = config["Vault"];
            if (!string.IsNullOrWhiteSpace(vault))
            {
                if (!vault.StartsWithI("http")) vault = $"https://{vault}.vault.azure.net/";

                var clientId = config["ClientId"];
                var clientSecret = config["ClientSecret"];
                var exceptions = new List<Exception>();
                if (string.IsNullOrWhiteSpace(clientId))
                    exceptions.Add(new ArgumentNullException("ClientId is missing"));

                if (string.IsNullOrWhiteSpace(clientSecret))
                    exceptions.Add(new ArgumentNullException("clientSecret is missing"));

                if (exceptions.Count > 0) throw new AggregateException(exceptions);

                configBbuilder.AddAzureKeyVault(vault, clientId, clientSecret);
            }

            /* make sure these files are loaded AFTER the vault, so their keys superseed the vaults' values - that way, unit tests will pass because the obfuscation key is whatever the appSettings says it is [and not a hidden secret inside the vault])  */
            if (Debugger.IsAttached || config.IsDevelopment())configBbuilder.AddJsonFile("appsettings.secret.json", true, true);

            // override using the azure environment variables into the configuration
            configBbuilder.AddEnvironmentVariables();
        }


        public static void ConfigureHostApplication(this IHostBuilder hostBuilder, IConfiguration appSettings=null)
        {
            //Set the console title to the application name
            Console.Title = appSettings[HostDefaults.ApplicationKey];

            //Add a handler for unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Encryption.SetDefaultEncryptionKey(appSettings["DefaultEncryptionKey"]);
            Encryption.EncryptEmails = appSettings.GetValueOrDefault("EncryptEmails", true);

            //Initialise the virtual date and time
            VirtualDateTime.Initialise(appSettings["DateTimeOffset"]);

            //Setup Threads
            appSettings.SetupThreads();

            hostBuilder.UseConsoleLifetime();
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