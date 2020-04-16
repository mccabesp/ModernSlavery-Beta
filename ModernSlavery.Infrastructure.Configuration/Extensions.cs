using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Extensions;

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
                        Environment.GetEnvironmentVariable("DEV_ENVIRONMENT").ToBoolean()) _EnvironmentName = "Local";
                    if (string.IsNullOrWhiteSpace(_EnvironmentName)) _EnvironmentName = "Local";
                }

                return _EnvironmentName;
        }

        public static void SetEnvironmentName(this IConfiguration configuration,string environmentName)
        {
            _EnvironmentName = environmentName;
            configuration[HostDefaults.EnvironmentKey] = environmentName;
        }

        public static bool IsLocal(this IConfiguration configuration)
        {
            return configuration.IsEnvironment("LOCAL");
        }

        public static bool IsDevelopment(this IConfiguration configuration)
        {
            return configuration.IsEnvironment("DEV", "DEVELOPMENT");
        }

        public static bool IsStaging(this IConfiguration configuration)
        {
            return configuration.IsEnvironment("STAGING");
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

        public static bool HasChildren(this IConfigurationSection section)
        {
            return section.GetChildren().Any();
        }

        public static IEnumerable<KeyValuePair<string, string>> GetChildValues(this IConfigurationSection parent)
        {
            var children = parent.GetChildren();

            if (!children.Any())
                yield return new KeyValuePair<string, string>(parent.Path, parent.Value);
            else
                foreach (var child in children)
                {
                    foreach (var childResult in child.GetChildValues())
                        yield return childResult;
                }
        }

        #endregion
    }
}
