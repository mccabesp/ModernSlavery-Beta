using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Options;

namespace ModernSlavery.WebUI.Shared.Options
{
    [Options("Features")]
    public class FeatureSwitchOptions : Dictionary<string, FeatureSwitchOptions.Feature>,IOptions
    {
        private readonly IConfiguration _configuration;

        public FeatureSwitchOptions(IConfiguration configuration) :base(StringComparer.OrdinalIgnoreCase)
        {
            _configuration = configuration;
        }

        #region Environment
        public string Environment => _configuration[HostDefaults.EnvironmentKey];

        public bool IsEnvironment(params string[] environmentNames)
        {
            return environmentNames.Any(en => Environment.EqualsI(en));
        }

        public bool IsDevelopment()
        {
            return IsEnvironment("Development");
        }

        public bool IsDev()
        {
            return IsEnvironment("DEV");
        }

        public bool IsQAT()
        {
            return IsEnvironment("QAT");
        }

        public bool IsPreProduction()
        {
            return IsEnvironment("PREPROD", "PREPRODUCTION");
        }

        public bool IsProduction()
        {
            return IsEnvironment("PROD", "PRODUCTION");
        }

        #endregion

        public bool IsDisabled(string featureName)
        {
            return !IsEnabled(featureName);
        }

        /// <summary>
        /// Checks if any features are disabled in priority order
        /// </summary>
        /// <param name="featureNames"></param>
        /// <returns></returns>
        public bool IsEnabled(params string[] featureNames)
        {
            if (featureNames == null || featureNames.Length == 0) throw new ArgumentNullException(nameof(featureNames));

            featureNames = featureNames.Distinct().ToArray();
            int i;
            for (i=0; i < featureNames.Length - 1; i++)
            {
                if (ContainsKey(featureNames[i])) return this[featureNames[i]].Enabled;
            }

            return FeatureIsEnabled(featureNames[i]);
        }

        private bool FeatureIsEnabled(string featureName)
        {
            var feature = ContainsKey(featureName) ? this[featureName] : null;

            return feature == null || feature.Enabled;
        }

        public class Feature
        {
            public enum Actions : byte
            {
                Enable,
                Disable
            }
            public Actions Action { get; set; }
            public DateTime StartDate { get; set; } = DateTime.MinValue;
            public DateTime EndDate { get; set; } = DateTime.MaxValue;

            public bool Enabled 
            {
                get 
                {
                    var now = VirtualDateTime.Now;
                    return Action == Actions.Disable ? now < StartDate || now > EndDate : now > StartDate && now < EndDate;
                }
            }
        }

        public void Validate() 
        {
            var exceptions = new List<Exception>();
            if (IsProduction() && IsEnabled("DevOps"))exceptions.Add(new ConfigurationErrorsException($"DevOps must not be enabled in the environment {Environment}"));
            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1) throw exceptions[0];
                throw new AggregateException(exceptions);
            }

        }
    }
}