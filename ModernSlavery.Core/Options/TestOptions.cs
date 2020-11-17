using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace ModernSlavery.Core.Options
{
    [Options("TestOptions")]
    public class TestOptions : IOptions
    {
        private readonly IConfiguration _configuration;

        public TestOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool ForceApplicationInsightsTracking { get; set; }
        public bool ForceGoogleAnalyticsTracking { get; set; }
        public bool StickySessions { get; set; } = true;
        public bool SkipSpamProtection { get; set; }
        public bool PinInPostTestMode { get; set; }
        public bool ShowEmailVerifyLink { get; set; }
        public string TestPrefix { get; set; }

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


        public void Validate()
        {
            var exceptions = new List<Exception>();
            //Check security settings for production environment
            if (IsProduction())
            {
                if (ShowEmailVerifyLink) exceptions.Add(new ConfigurationErrorsException("ShowEmailVerifyLink is not permitted in Production environment"));
                if (PinInPostTestMode) exceptions.Add(new ConfigurationErrorsException("PinInPostTestMode is not permitted in Production environment"));
                if (SkipSpamProtection) exceptions.Add(new ConfigurationErrorsException("SkipSpamProtection is not permitted in Production environment"));
            }

            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1) throw exceptions[0];
                throw new AggregateException(exceptions);
            }
        }
    }
}