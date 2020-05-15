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
        private IConfigurationBuilder _builder;
        private IConfiguration _config = null;
        private bool _built=false;

        private Dictionary<string, string> _hostSettings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public ConfigBuilder(string applicationName = null, string contentRoot = null, Dictionary<string, string> additionalSettings = null, params string[] commandlineArgs)
        {
            _builder = new ConfigurationBuilder();

            //Add a source for host settings
            _builder.AddInMemoryCollection(_hostSettings);

            //Set the host application name 
            if (string.IsNullOrWhiteSpace(applicationName)) applicationName = Assembly.GetEntryAssembly().GetName().Name;
            AddApplicationName(applicationName);

            //Set the host content root
            if (string.IsNullOrWhiteSpace(contentRoot)) contentRoot=AppContext.BaseDirectory;
            AddContentRoot(contentRoot);

            //Set the host environment name from the environment variables
            AddEnvironment(GetEnvironmentName());

            //Add any additional settings source
            if (additionalSettings != null) AddSettings(additionalSettings);

            //Add any commaind line settings
            if (commandlineArgs!=null && commandlineArgs.Any()) _builder.AddCommandLine(commandlineArgs);

            CreateSources();
        }


        public void Dispose()
        {
            _builder = null;
            _config = null;
            _hostSettings = null;
        }

        public IConfiguration Build()
        {
            if (_built) return _config;

            _config = _builder.Build();

            _built = true;

            //Make sure we know the environment
            var environmentName = _config[HostDefaults.EnvironmentKey];
            if (string.IsNullOrWhiteSpace(environmentName)) throw new ArgumentNullException(nameof(environmentName));

            Console.WriteLine($"Environment: {environmentName}");

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

            _builder.SetFileProvider(new PhysicalFileProvider(appSettingsPath));
            
            _builder.AddJsonFile("appsettings.json", false, true);
            _builder.AddJsonFile($"appsettings.{environmentName}.json", true, true);

            _builder.AddEnvironmentVariables();
            _built = false;

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

                _builder.AddAzureKeyVault(vault, clientId, clientSecret);
            }

            /* make sure these files are loaded AFTER the vault, so their keys superseed the vaults' values - that way, unit tests will pass because the obfuscation key is whatever the appSettings says it is [and not a hidden secret inside the vault])  */
            if (Debugger.IsAttached || _config.IsLocal())
            {
                var appAssembly = Misc.GetTopAssembly();
                if (appAssembly != null) _builder.AddUserSecrets(appAssembly, true);

                _builder.AddJsonFile("appsettings.secret.json", true, true);
            }

            _builder.AddJsonFile("appsettings.unittests.json", true, false);

            // override using the azure environment variables into the configuration
            _builder.AddEnvironmentVariables();

            _built = false;
        }

        private string GetEnvironmentName()
        {
            string environmentName = _config?[HostDefaults.EnvironmentKey];
            if (string.IsNullOrWhiteSpace(environmentName))
            {
                environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (string.IsNullOrWhiteSpace(environmentName))
                    environmentName = Environment.GetEnvironmentVariable("ASPNET_ENV");
                if (string.IsNullOrWhiteSpace(environmentName))
                    environmentName = Environment.GetEnvironmentVariable("Hosting:Environment");
                if (string.IsNullOrWhiteSpace(environmentName))
                    environmentName = Environment.GetEnvironmentVariable("AzureWebJobsEnv");
                if (string.IsNullOrWhiteSpace(environmentName))environmentName =Environment.GetEnvironmentVariable("Environment"); //This is used by webjobs SDK v3 
                if (string.IsNullOrWhiteSpace(environmentName) && Environment.GetEnvironmentVariable("DEV_ENVIRONMENT").ToBoolean()) environmentName = "Development";
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
                _builder.AddCommandLine(commandlineArgs);
                _built = false;
            };
        }

        public void AddEnvironment(string environmentName)
        {
            if (string.IsNullOrWhiteSpace(environmentName)) throw new ArgumentNullException(nameof(environmentName));
            _hostSettings[HostDefaults.EnvironmentKey] = environmentName;
            _built = false;
        }

        public void AddApplicationName(string applicationName)
        {
            if (string.IsNullOrWhiteSpace(applicationName)) throw new ArgumentNullException(nameof(applicationName));
            _hostSettings[HostDefaults.ApplicationKey] = applicationName;
            _built = false;
        }

        public void AddContentRoot(string contentRoot)
        {
            if (string.IsNullOrWhiteSpace(contentRoot)) throw new ArgumentNullException(nameof(contentRoot));
            _hostSettings[HostDefaults.ContentRootKey] = contentRoot;
            _built = false;
        }

        public void AddSetting(string key,string value)
        {
            _hostSettings[key] = value;
            _built = false;
        }

        public void AddSettings(Dictionary<string, string> additionalSettings)
        {
            if (additionalSettings != null) 
            {
                _builder.AddInMemoryCollection(additionalSettings);
                _built = false;
            };
        }

        public void ConfigureHost(IHostBuilder hostBuilder)
        {
            Build();

            hostBuilder.ConfigureHostConfiguration(configBuilder =>
            {
                //configBuilder.AddConfiguration(_config);

                configBuilder.SetFileProvider(_builder.GetFileProvider());
                _builder.Sources.ForEach(s => configBuilder.Sources.Add(s));
            });
        }
    }
}