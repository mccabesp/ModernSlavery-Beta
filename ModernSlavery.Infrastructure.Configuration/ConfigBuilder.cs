﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Infrastructure.Configuration
{
    public class ConfigBuilder: IDisposable
    {
        private Dictionary<string, string> _additionalSettings;
        private string[] _commandlineArgs;

        private IConfiguration _appConfig;

        public ConfigBuilder(Dictionary<string, string> additionalSettings = null, params string[] commandlineArgs)
        {
            _additionalSettings = additionalSettings ?? new Dictionary<string, string>();
            _commandlineArgs = commandlineArgs;

        }

        public void Dispose()
        {
            _additionalSettings = null;
        }

        public IConfiguration Build()
        {
            var appBuilder = new ConfigurationBuilder();
            appBuilder.AddEnvironmentVariables("DOTNET_");
            appBuilder.AddEnvironmentVariables("ASPNETCORE_");
            appBuilder.AddEnvironmentVariables();
            if (_commandlineArgs!=null && _commandlineArgs.Any())appBuilder.AddCommandLine(_commandlineArgs);

            _appConfig = appBuilder.Build();

            if (_appConfig["WEBSITE_RUN_FROM_PACKAGE"] == "1")
            {
                var appData = _appConfig["HOME"];
                if (!string.IsNullOrWhiteSpace(appData))
                {
                    Directory.SetCurrentDirectory(appData);
                    Console.WriteLine($"CurrentDirectory set to [HOME]:{appData} when [WEBSITE_RUN_FROM_PACKAGE]=1");
                }
                else
                {
                    Console.WriteLine($"Cannot set CurrentDirectory to empty [HOME] is empty when [WEBSITE_RUN_FROM_PACKAGE]=1");
                }
            }

            //Make sure we know the environment
            var environmentName = GetEnvironmentName();
                if (string.IsNullOrWhiteSpace(environmentName)) throw new ArgumentNullException(nameof(environmentName));

            //Set the location of the appsettings.json files
            appBuilder.SetFileProvider(new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory));

            //Add the root and environment appsettings files
            appBuilder.EnsureJsonFiles(environmentName, Debugger.IsAttached || _appConfig.IsDevelopment() || _appConfig.IsTest());
            _appConfig = appBuilder.Build();

            //Add the azure key vault to configuration
            var vault = _appConfig["Vault"];
            if (!string.IsNullOrWhiteSpace(vault))
            {
                if (!vault.StartsWithI("http")) vault = $"https://{vault}.vault.azure.net/";

                var clientId = _appConfig["ClientId"];
                var clientSecret = _appConfig["ClientSecret"];

                if (string.IsNullOrWhiteSpace(clientId) && string.IsNullOrWhiteSpace(clientSecret))
                {
                    //Create Managed Service Identity token provider
                    var azureServiceTokenProvider = new AzureServiceTokenProvider();
                    //Create the Key Vault client
                    var kvClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                    
                    //Add Key Vault to configuration pipeline
                    appBuilder.AddAzureKeyVault(vault, kvClient, new DefaultKeyVaultSecretManager());
                }
                else if (string.IsNullOrWhiteSpace(clientId))
                    throw new ArgumentNullException("ClientId is missing");

                else if (string.IsNullOrWhiteSpace(clientSecret))
                    throw new ArgumentNullException("ClientSecret is missing");
                else
                    appBuilder.AddAzureKeyVault(vault, clientId, clientSecret);

                Console.WriteLine($@"Using KeyVault: {vault}");
            }

            /* make sure these files are loaded AFTER the vault, so their keys superseed the vaults' values - that way, unit tests will pass because the obfuscation key is whatever the appSettings says it is [and not a hidden secret inside the vault])  */
            if (Debugger.IsAttached || _appConfig.IsDevelopment() || _appConfig.IsTest())
            {
                appBuilder.PromoteConfigSecretSources();
            }

            //Add any additional settings source
            if (_additionalSettings != null && _additionalSettings.Any())
                appBuilder.AddInMemoryCollection(_additionalSettings);

            // override using the azure environment variables into the configuration
            appBuilder.PromoteConfigSources<EnvironmentVariablesConfigurationSource>();

            _appConfig = appBuilder.Build();

            //Ensure we have an application name
            if (string.IsNullOrWhiteSpace(_appConfig[HostDefaults.ApplicationKey]))
                _appConfig[HostDefaults.ApplicationKey] = Assembly.GetEntryAssembly().GetName().Name;

            _appConfig[HostDefaults.EnvironmentKey] = environmentName;

            //Resolve all the variable names in the configuration
            var configDictionary=_appConfig.ResolveVariableNames();

            //Dump the settings to the console
            if (_appConfig.GetValueOrDefault("TestOptions:DUMP_SETTINGS", false))
            {
                var slotName = _appConfig.GetValueOrDefault("DEPLOYMENT_SLOT_NAME", "");//Used to differentiate between production and staging slots
                if (!string.IsNullOrWhiteSpace(slotName)) slotName = "-" + slotName.TrimNonLettersOrDigits();
                var dumpSettingPath = Path.Combine(_appConfig["Filepaths:LogFiles"], $"{_appConfig.GetApplicationName()}{slotName}.SETTINGS.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(dumpSettingPath));
                File.WriteAllLines(dumpSettingPath, configDictionary.Keys.Select(key=>$@"[{key}]={configDictionary[key]}"));
                Console.WriteLine($@"AppSettings Dumped to file: {dumpSettingPath}");
                _appConfig["DumpSettingPath"] = dumpSettingPath;
            }

            return _appConfig;
        }

        private string GetEnvironmentName()
        {
            var environmentName = _appConfig[HostDefaults.EnvironmentKey];

            if (string.IsNullOrWhiteSpace(environmentName))
                environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
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