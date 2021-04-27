using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Extensions;
using Newtonsoft.Json.Linq;

namespace ModernSlavery.Infrastructure.Configuration
{
    public static class Extensions
    {
        #region Environment

        internal static string _EnvironmentName;

        private static string GetEnvironmentName(this IConfiguration configuration)
        {
            if (_EnvironmentName == null)
            {
                _EnvironmentName = configuration?[HostDefaults.EnvironmentKey];
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
                    Environment.GetEnvironmentVariable("DEV_ENVIRONMENT").ToBoolean()) _EnvironmentName = "Development";
                if (string.IsNullOrWhiteSpace(_EnvironmentName)) _EnvironmentName = "Development";
            }

            return _EnvironmentName;
        }

        public static void SetEnvironmentName(this IConfiguration configuration, string environmentName)
        {
            _EnvironmentName = environmentName;
            configuration[HostDefaults.EnvironmentKey] = environmentName;
        }

        public static bool IsDevelopment(this IConfiguration configuration)
        {
            return configuration.IsEnvironment("Development");
        }

        public static bool IsTest(this IConfiguration configuration)
        {
            return configuration.IsEnvironment("Test");
        }

        public static bool IsDev(this IConfiguration configuration)
        {
            return configuration.IsEnvironment("DEV");
        }

        public static bool IsQAT(this IConfiguration configuration)
        {
            return configuration.IsEnvironment("QAT");
        }

        public static bool IsPreProduction(this IConfiguration configuration)
        {
            return configuration.IsEnvironment("PREPROD", "PREPRODUCTION");
        }

        public static bool IsProduction(this IConfiguration configuration)
        {
            return configuration.IsEnvironment("PROD", "PRODUCTION");
        }

        public static bool IsEnvironment(this IConfiguration configuration, params string[] environmentNames)
        {
            var configEnvironment = configuration.GetEnvironmentName();
            foreach (var environmentName in environmentNames)
                if (configEnvironment.EqualsI(environmentName))
                    return true;

            return false;
        }
        #endregion




        public static bool ContainsSecretFiles(this IConfiguration config)
        {
            var configRoot = (Microsoft.Extensions.Configuration.ConfigurationRoot)config;
            return configRoot.Providers.OfType<FileConfigurationProvider>().Any(p => p.Source.Path.ContainsI(".secret."));
        }


        public static Dictionary<string, string> LoadSettings(string filepath, string sectionName = null)
        {
            if (string.IsNullOrWhiteSpace(filepath)) throw new ArgumentNullException(nameof(filepath));
            if (!File.Exists(filepath)) throw new FileNotFoundException("Cannot find file", filepath);
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile(filepath);
            var config = configBuilder.Build();
            return config.ToDictionary(sectionName);
        }
        public static void SaveSettings(this Dictionary<string, string> settings, string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath)) throw new ArgumentNullException(nameof(filepath));

            JObject jsonObj = File.Exists(filepath) ? Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(File.ReadAllText(filepath)) : new JObject();

            foreach (var key in settings.Keys)
                SetValueRecursively(key, jsonObj, settings[key]);

            string output = Json.SerializeObject(jsonObj, indented:true);

            File.WriteAllText(filepath, output);
        }

        private static void SetValueRecursively<T>(string sectionPathKey, dynamic jsonObj, T value)
        {
            // split the string at the first ':' character
            var remainingSections = sectionPathKey.Split(":", 2);

            var currentSection = remainingSections[0];
            if (remainingSections.Length > 1)
            {
                // continue with the procress, moving down the tree
                var nextSection = remainingSections[1];
                if (jsonObj[currentSection] == null) jsonObj[currentSection] = new JObject();
                SetValueRecursively(nextSection, jsonObj[currentSection], value);
            }
            else
            {
                // we've got to the end of the tree, set the value
                jsonObj[currentSection] = value;
            }
        }

        //Promotes previous configuration sources to top of stack
        public static void EnsureJsonFile(this IConfigurationBuilder configBuilder, string path, bool optional, bool reloadOnChange)
        {
            var sources = configBuilder.Sources.OfType<JsonConfigurationSource>().ToList();
            if (Path.IsPathRooted(path))
            {
                var provider = configBuilder.GetFileProvider() as PhysicalFileProvider;

                path = Path.GetRelativePath(provider.Root,path);
            }

            var source = sources.FirstOrDefault(s => s.Path == path);
            if (source == null)
                configBuilder.AddJsonFile(path, optional, reloadOnChange);
            else
            {
                source.Optional = optional;
                source.ReloadOnChange = reloadOnChange;
            }
        }


        public static void EnsureJsonFiles(this IConfigurationBuilder configBuilder, string environment, bool includeSecrets = false)
        {
            var configLoaded = configBuilder.Sources.OfType<JsonConfigurationSource>().Any(s => s.Path.EqualsI("appsettings.json"));
            if (configLoaded) throw new Exception("Attempt to load appsettings which are already loaded");

            //Register the top settings file
            configBuilder.EnsureJsonFile("appsettings.json", false, false);

            //Register the top environment settings file
            configBuilder.EnsureJsonFile($"appsettings.{environment}.json", true, false);

            var provider = configBuilder.GetFileProvider() as PhysicalFileProvider;

            var directory = new DirectoryInfo(Path.Combine(provider.Root, "App_Settings"));

            IEnumerable<FileInfo> topSecretSettings=null;
            IEnumerable<FileInfo> environmentSecretSettings = null;
            if (directory.Exists)
            {
                //Get all json files in App_Settings folder
                var allSettings = directory.GetFiles("*.json", SearchOption.AllDirectories);

                //Register the directory settings files
                var topSettings = allSettings.Where(f => Path.GetFileNameWithoutExtension(f.Name).IndexOf('.') == -1);
                foreach (var file in topSettings)
                    configBuilder.EnsureJsonFile(file.FullName, false, false);
                allSettings = allSettings.Except(topSettings).ToArray();

                //Register the directory environment settings files
                var environmentSettings = allSettings.Where(f => f.Name.EndsWithI($".{environment}.json"));
                foreach (var file in environmentSettings)
                    configBuilder.EnsureJsonFile(file.FullName, false, false);
                allSettings = allSettings.Except(environmentSettings).ToArray();

                //Register the directory secret files
                environmentSecretSettings = allSettings.Where(f => f.Name.EndsWithI($".{environment}.secret.json"));
                allSettings = allSettings.Except(environmentSecretSettings).ToArray();

                //Register the directory environment secret files
                topSecretSettings = allSettings.Where(f => f.Name.EndsWithI($".secret.json"));
            }

            if (includeSecrets)
            {
                //Register the top secret file
                configBuilder.EnsureJsonFile($"appsettings.secret.json", true, false);
                
                //Register the top environment settings file
                configBuilder.EnsureJsonFile($"appsettings.{environment}.secret.json", true, false);

                //Register the directory secret files
                if (topSecretSettings!=null)
                    foreach (var file in topSecretSettings)
                        configBuilder.EnsureJsonFile(file.FullName, false, false);

                //Register the directory environment secret files
                if (environmentSecretSettings != null)
                    foreach (var file in environmentSecretSettings)
                        configBuilder.EnsureJsonFile(file.FullName, false, false);
            }
        }

        //Promotes previous configuration sources to top of stack
        public static void PromoteConfigSources<T>(this IConfigurationBuilder configBuilder) where T : IConfigurationSource
        {
            var sources = configBuilder.Sources.OfType<T>().ToList();
            sources.ForEach(s => configBuilder.Sources.Remove(s));
            sources.ForEach(s => configBuilder.Sources.Add(s));
        }

        //Promotes previous configuration sources to top of stack
        public static void PromoteConfigSecretSources(this IConfigurationBuilder configBuilder)
        {
            var sources = configBuilder.Sources.OfType<JsonConfigurationSource>().Where(s => s.Path.ContainsI(".secret")).ToList();
            sources.ForEach(s => configBuilder.Sources.Remove(s));
            sources.ForEach(s => configBuilder.Sources.Add(s));
        }

        
    }
}
