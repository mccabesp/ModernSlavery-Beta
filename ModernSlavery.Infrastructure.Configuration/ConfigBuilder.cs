using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Infrastructure.Configuration
{
    public class ConfigBuilder
    {
        private readonly IConfigurationBuilder _configBuilder;
        private readonly string _environmentName;
        private readonly Dictionary<string, string> _additionalSettings;

        public ConfigBuilder(IConfigurationBuilder configBuilder, string environmentName, Dictionary<string, string> additionalSettings = null)
        {
            _configBuilder = configBuilder ?? new ConfigurationBuilder();
            if (string.IsNullOrWhiteSpace(environmentName)) throw new ArgumentNullException(nameof(environmentName));
            _environmentName = environmentName;
            _additionalSettings = additionalSettings;
        }

        public IConfiguration Configuration { get; private set; }


        public IConfiguration Build()
        {
            Console.WriteLine($"Environment: {_environmentName}");

            _configBuilder.AddJsonFile("appsettings.json", false, true);
            _configBuilder.AddJsonFile($"appsettings.{_environmentName}.json", true, true);

            _configBuilder.AddEnvironmentVariables();

            if (_additionalSettings != null && _additionalSettings.Any())
                _configBuilder.AddInMemoryCollection(_additionalSettings);

            var configuration = _configBuilder.Build();

            //Add the azure key vault to configuration
            var vault = configuration["Vault"];
            if (!string.IsNullOrWhiteSpace(vault))
            {
                if (!vault.StartsWithI("http")) vault = $"https://{vault}.vault.azure.net/";

                var clientId = configuration["ClientId"];
                var clientSecret = configuration["ClientSecret"];
                var exceptions = new List<Exception>();
                if (string.IsNullOrWhiteSpace(clientId))
                    exceptions.Add(new ArgumentNullException("ClientId is missing"));

                if (string.IsNullOrWhiteSpace(clientSecret))
                    exceptions.Add(new ArgumentNullException("clientSecret is missing"));

                if (exceptions.Count > 0) throw new AggregateException(exceptions);

                _configBuilder.AddAzureKeyVault(vault, clientId, clientSecret);
            }

            /* make sure these files are loaded AFTER the vault, so their keys superseed the vaults' values - that way, unit tests will pass because the obfuscation key is whatever the appSettings says it is [and not a hidden secret inside the vault])  */
            if (Debugger.IsAttached || IsEnvironment("Local"))
            {
                var appAssembly = Misc.GetTopAssembly();
                if (appAssembly != null) _configBuilder.AddUserSecrets(appAssembly, true);

                _configBuilder.AddJsonFile("appsettings.secret.json", true, true);
            }

            _configBuilder.AddJsonFile("appsettings.unittests.json", true, false);

            // override using the azure environment variables into the configuration
            _configBuilder.AddEnvironmentVariables();
            var config = _configBuilder.Build();

            if (!IsProduction() && config.GetValue<bool>("DUMP_APPSETTINGS"))
                foreach (var key in GetKeys(config))
                    Console.WriteLine($@"APPSETTING[""{key}""]={config[key]}");

            Encryption.SetDefaultEncryptionKey(config["DefaultEncryptionKey"]);
            Encryption.EncryptEmails = config.GetValueOrDefault("EncryptEmails", true);

            //Initialise the virtual date and time
            VirtualDateTime.Initialise(config["DateTimeOffset"]);

            return config;
        }

        private IEnumerable<string> GetKeys(IConfiguration section)
        {
            return section.GetChildren().Select(c => c.Key);
        }

        #region Environment

        private string _EnvironmentName;

        public string EnvironmentName
        {
            get
            {
                if (_EnvironmentName == null)
                {
                    _EnvironmentName = Configuration?[HostDefaults.EnvironmentKey];
                    if (string.IsNullOrWhiteSpace(_EnvironmentName))
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

        public bool IsLocal()
        {
            return IsEnvironment("LOCAL");
        }

        public bool IsDevelopment()
        {
            return IsEnvironment("DEV", "DEVELOPMENT");
        }

        public bool IsStaging()
        {
            return IsEnvironment("STAGING");
        }

        public bool IsPreProduction()
        {
            return IsEnvironment("PREPROD", "PREPRODUCTION");
        }

        public bool IsProduction()
        {
            return IsEnvironment("PROD", "PRODUCTION");
        }

        public bool IsEnvironment(params string[] environmentNames)
        {
            foreach (var environmentName in environmentNames)
                if (EnvironmentName.EqualsI(environmentName))
                    return true;

            return false;
        }

        #endregion
    }
}