using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using ModernSlavery.Extensions;

namespace ModernSlavery.Infrastructure.Configuration
{
    public interface IConfigBuilder
    {
        IConfiguration Build(Dictionary<string, string> additionalSettings = null);
    }

    public class ConfigBuilder: IConfigBuilder
    {
        public IConfiguration Configuration { get; private set; }
        private readonly IConfigurationBuilder _Builder;
        public ConfigBuilder(IConfigurationBuilder configBuilder = null)
        {
            _Builder = configBuilder ?? new ConfigurationBuilder();
        }

        #region Environment
        private string _EnvironmentName;
        public string EnvironmentName
        {
            get
            {
                if (_EnvironmentName == null)
                {
                    _EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    if (string.IsNullOrWhiteSpace(_EnvironmentName))_EnvironmentName = Environment.GetEnvironmentVariable("ASPNET_ENV");
                    if (string.IsNullOrWhiteSpace(_EnvironmentName))_EnvironmentName = Environment.GetEnvironmentVariable("Hosting:Environment");
                    if (string.IsNullOrWhiteSpace(_EnvironmentName))_EnvironmentName = Environment.GetEnvironmentVariable("AzureWebJobsEnv");
                    if (string.IsNullOrWhiteSpace(_EnvironmentName))_EnvironmentName = Environment.GetEnvironmentVariable("Environment"); //This is used by webjobs SDK v3 
                    if (string.IsNullOrWhiteSpace(_EnvironmentName) && Environment.GetEnvironmentVariable("DEV_ENVIRONMENT").ToBoolean())_EnvironmentName = "Local";
                    if (string.IsNullOrWhiteSpace(_EnvironmentName))_EnvironmentName = "Local";
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
            foreach (string environmentName in environmentNames)
            {
                if (EnvironmentName.EqualsI(environmentName))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion


        public IConfiguration Build(Dictionary<string, string> additionalSettings = null)
        {
            Console.WriteLine($"Environment: {EnvironmentName}");

            _Builder.AddJsonFile("appsettings.json", false, true);
            _Builder.AddJsonFile($"appsettings.{EnvironmentName}.json", true, true);

            _Builder.AddEnvironmentVariables();

            if (additionalSettings != null && additionalSettings.Any())
            {
                _Builder.AddInMemoryCollection(additionalSettings);
            }

            IConfigurationRoot configuration = _Builder.Build();

            //Add the azure key vault to configuration
            string vault = configuration["Vault"];
            if (!string.IsNullOrWhiteSpace(vault))
            {
                if (!vault.StartsWithI("http"))
                {
                    vault = $"https://{vault}.vault.azure.net/";
                }

                string clientId = configuration["ClientId"];
                string clientSecret = configuration["ClientSecret"];
                var exceptions = new List<Exception>();
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    exceptions.Add(new ArgumentNullException("ClientId is missing"));
                }

                if (string.IsNullOrWhiteSpace(clientSecret))
                {
                    exceptions.Add(new ArgumentNullException("clientSecret is missing"));
                }

                if (exceptions.Count > 0)
                {
                    throw new AggregateException(exceptions);
                }

                _Builder.AddAzureKeyVault(vault, clientId, clientSecret);
            }

            /* make sure these files are loaded AFTER the vault, so their keys superseed the vaults' values - that way, unit tests will pass because the obfuscation key is whatever the appSettings says it is [and not a hidden secret inside the vault])  */
            if (Debugger.IsAttached || IsEnvironment("Local"))
            {
                Assembly appAssembly = Misc.GetTopAssembly();
                if (appAssembly != null)
                {
                    _Builder.AddUserSecrets(appAssembly, true);
                }

                _Builder.AddJsonFile("appsettings.secret.json", true, true);
            }

            _Builder.AddJsonFile("appsettings.unittests.json", true, false);

            // override using the azure environment variables into the configuration
            _Builder.AddEnvironmentVariables();
            var config = _Builder.Build();

            if (!IsProduction() && config.GetValue<bool>("DUMP_APPSETTINGS"))
            {
                foreach (string key in GetKeys(config))
                    Console.WriteLine($@"APPSETTING[""{key}""]={config[key]}");
            }

            return config;
        }

        private IEnumerable<string> GetKeys(IConfiguration section)
        {
            return section.GetChildren().Select(c => c.Key);
        }

    }

}
