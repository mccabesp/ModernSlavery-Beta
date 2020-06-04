using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Infrastructure.Configuration
{
    public class ConfigBuilder: IDisposable
    {
        Dictionary<string, string> _additionalSettings = null; 

        public ConfigBuilder(Dictionary<string, string> additionalSettings = null)
        {
            _additionalSettings = additionalSettings;
        }

        public void Dispose()
        {
            _additionalSettings = null;
        }

        public IConfiguration Build()
        {
            var appBuilder = new ConfigurationBuilder();
            //Make sure we know the environment
            var environmentName = GetEnvironmentName();
                if (string.IsNullOrWhiteSpace(environmentName)) throw new ArgumentNullException(nameof(environmentName));

            //Set the location of the appsettings.json files
            appBuilder.SetFileProvider(new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory));

            //Add the root and environment appsettings files
            appBuilder.AddJsonFile("appsettings.json", false, true);
            appBuilder.AddJsonFile($"appsettings.{environmentName}.json", true, true);

            var _appConfig = appBuilder.Build();

            //Add the azure key vault to configuration
            var vault = _appConfig["Vault"];
            if (!string.IsNullOrWhiteSpace(vault))
            {
                if (!vault.StartsWithI("http")) vault = $"https://{vault}.vault.azure.net/";

                var clientId = _appConfig["ClientId"];
                var clientSecret = _appConfig["ClientSecret"];
                var exceptions = new List<Exception>();
                if (string.IsNullOrWhiteSpace(clientId))
                    exceptions.Add(new ArgumentNullException("ClientId is missing"));

                if (string.IsNullOrWhiteSpace(clientSecret))
                    exceptions.Add(new ArgumentNullException("clientSecret is missing"));

                if (exceptions.Count > 0) throw new AggregateException(exceptions);

                appBuilder.AddAzureKeyVault(vault, clientId, clientSecret);
            }

            /* make sure these files are loaded AFTER the vault, so their keys superseed the vaults' values - that way, unit tests will pass because the obfuscation key is whatever the appSettings says it is [and not a hidden secret inside the vault])  */
            if (Debugger.IsAttached || _appConfig.IsDevelopment())
            {
                var appAssembly = Misc.GetTopAssembly();
                if (appAssembly != null) appBuilder.AddUserSecrets(appAssembly, true);

                appBuilder.AddJsonFile("appsettings.secret.json", true, true);
            }

            // override using the azure environment variables into the configuration
            appBuilder.AddEnvironmentVariables();

            //Add any additional settings source
            if (_additionalSettings != null && _additionalSettings.Any())
                appBuilder.AddInMemoryCollection(_additionalSettings);

            _appConfig = appBuilder.Build();

            _appConfig[HostDefaults.EnvironmentKey] = environmentName;

            //Dump the settings to the console
            if (!_appConfig.IsProduction() && _appConfig.GetValueOrDefault("DUMP_SETTINGS", false))
            {
                //Console.WriteLine(_appConfig.GetDebugView());
                foreach (var key in _appConfig.GetChildren().Select(c => c.Key))
                    Console.WriteLine($@"SETTING[""{key}""]={_appConfig[key]}");
            }

            return _appConfig;
        }

        private string GetEnvironmentName()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(environmentName))
                environmentName = Environment.GetEnvironmentVariable("ASPNET_ENV");
            if (string.IsNullOrWhiteSpace(environmentName))
                environmentName = Environment.GetEnvironmentVariable("Hosting:Environment");
            if (string.IsNullOrWhiteSpace(environmentName))
                environmentName = Environment.GetEnvironmentVariable("AzureWebJobsEnv");
            if (string.IsNullOrWhiteSpace(environmentName))environmentName =Environment.GetEnvironmentVariable("Environment"); //This is used by webjobs SDK v3 
            if (string.IsNullOrWhiteSpace(environmentName) && Environment.GetEnvironmentVariable("DEV_ENVIRONMENT").ToBoolean()) environmentName = "Development";
            return environmentName;
        }

    }
}