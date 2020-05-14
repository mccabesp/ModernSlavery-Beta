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
    public class ConfigBuilder
    {
        public readonly IConfigurationBuilder Builder;
        private IConfiguration _config = null;
        private bool built=false;

        private Dictionary<string, string> _additionalSettings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public ConfigBuilder(string environmentName = null)
        {
            Builder = new ConfigurationBuilder();
            Builder.AddEnvironmentVariables(prefix: "DOTNET_");

            //Get the name from the environment
            if (!string.IsNullOrWhiteSpace(environmentName)) environmentName=GetEnvironmentName();
            if (!string.IsNullOrWhiteSpace(environmentName))AddEnvironment(environmentName);

            AddApplicationName(Assembly.GetEntryAssembly().GetName().Name);

            AddContentRoot(AppContext.BaseDirectory);

            CreateSources();
        }

        public IConfiguration Build()
        {
            if (built) return _config;

            _config = Builder.Build();
            built = true;

            Console.WriteLine($"Environment: {_config[HostDefaults.EnvironmentKey]}");

            if (!_config.IsProduction() && _config.GetValue<bool>("DUMP_APPSETTINGS"))
                foreach (var key in GetKeys(_config))
                    Console.WriteLine($@"APPSETTING[""{key}""]={_config[key]}");

            return _config;
        }

        private void CreateSources()
        {
            Build();

            //Make sure we know the environment
            var environmentName = _config[HostDefaults.EnvironmentKey];
            if (string.IsNullOrWhiteSpace(environmentName)) throw new ArgumentNullException(nameof(environmentName));

            string appSettingsPath = AppDomain.CurrentDomain.BaseDirectory;

            Builder.SetFileProvider(new PhysicalFileProvider(appSettingsPath));
            
            Builder.AddJsonFile("appsettings.json", false, true);
            Builder.AddJsonFile($"appsettings.{environmentName}.json", true, true);

            Builder.AddEnvironmentVariables();
            built = false;

            Build();

            //Add the azure key vault to configuration
            var vault = _config["Vault"];
            if (!string.IsNullOrWhiteSpace(vault))
            {
                if (!vault.StartsWithI("http")) vault = $"https://{vault}.vault.azure.net/";

                var clientId = _config["ClientId"];
                var clientSecret = _config["ClientSecret"];
                var exceptions = new List<Exception>();
                if (string.IsNullOrWhiteSpace(clientId))
                    exceptions.Add(new ArgumentNullException("ClientId is missing"));

                if (string.IsNullOrWhiteSpace(clientSecret))
                    exceptions.Add(new ArgumentNullException("clientSecret is missing"));

                if (exceptions.Count > 0) throw new AggregateException(exceptions);

                Builder.AddAzureKeyVault(vault, clientId, clientSecret);
            }

            /* make sure these files are loaded AFTER the vault, so their keys superseed the vaults' values - that way, unit tests will pass because the obfuscation key is whatever the appSettings says it is [and not a hidden secret inside the vault])  */
            if (Debugger.IsAttached || _config.IsLocal())
            {
                var appAssembly = Misc.GetTopAssembly();
                if (appAssembly != null) Builder.AddUserSecrets(appAssembly, true);

                Builder.AddJsonFile("appsettings.secret.json", true, true);
            }

            Builder.AddJsonFile("appsettings.unittests.json", true, false);

            // override using the azure environment variables into the configuration
            Builder.AddEnvironmentVariables();

            built = false;
        }

        private string GetEnvironmentName()
        {
            string environmentName = _config[HostDefaults.EnvironmentKey];
            if (environmentName == null)
            {
                environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (string.IsNullOrWhiteSpace(environmentName))
                    environmentName = Environment.GetEnvironmentVariable("ASPNET_ENV");
                if (string.IsNullOrWhiteSpace(environmentName))
                    environmentName = Environment.GetEnvironmentVariable("Hosting:Environment");
                if (string.IsNullOrWhiteSpace(environmentName))
                    environmentName = Environment.GetEnvironmentVariable("AzureWebJobsEnv");
                if (string.IsNullOrWhiteSpace(environmentName))environmentName =Environment.GetEnvironmentVariable("Environment"); //This is used by webjobs SDK v3 
                if (string.IsNullOrWhiteSpace(environmentName) && Environment.GetEnvironmentVariable("DEV_ENVIRONMENT").ToBoolean()) environmentName = "Deveopment";
            }
            return environmentName;
        }

        private IEnumerable<string> GetKeys(IConfiguration section)
        {
            return section.GetChildren().Select(c => c.Key);
        }

        public void AddCommandLineArgs(string[] commandlineArgs)
        {
            if (commandlineArgs != null && commandlineArgs.Any()) {
                Builder.AddCommandLine(commandlineArgs);
                built = false;
            };
        }

        public void AddEnvironment(string environmentName)
        {
            if (string.IsNullOrWhiteSpace(environmentName)) throw new ArgumentNullException(nameof(environmentName));
            _additionalSettings[HostDefaults.EnvironmentKey] = environmentName;
            built = false;
        }

        public void AddApplicationName(string applicationName)
        {
            if (string.IsNullOrWhiteSpace(applicationName)) throw new ArgumentNullException(nameof(applicationName));
            _additionalSettings[HostDefaults.ApplicationKey] = applicationName;
            built = false;
        }

        public void AddContentRoot(string contentRoot)
        {
            if (string.IsNullOrWhiteSpace(contentRoot)) throw new ArgumentNullException(nameof(contentRoot));
            _additionalSettings[HostDefaults.ContentRootKey] = contentRoot;
            built = false;
        }

        public void AddSetting(string key,string value)
        {
            _additionalSettings[key] = value;
            built = false;
        }

        public void AddSettings(Dictionary<string, string> additionalSettings)
        {
            if (additionalSettings != null) additionalSettings.ForEach(kv => {
                _additionalSettings[kv.Key] = kv.Value;
                built = false;
            });
        }

        public void ConfigureHost(IHostBuilder hostBuilder)
        {
            Build();

            hostBuilder.ConfigureHostConfiguration(configBuilder =>
            {
                //configBuilder.AddConfiguration(_config);

                configBuilder.SetFileProvider(Builder.GetFileProvider());
                Builder.Sources.ForEach(s => configBuilder.Sources.Add(s));
            });
        }
    }
}