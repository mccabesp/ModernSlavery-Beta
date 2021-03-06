﻿using Microsoft.Extensions.Configuration;
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
        private readonly FeatureSwitchOptions _featureSwitchOptions;

        public TestOptions(IConfiguration configuration, FeatureSwitchOptions featureSwitchOptions)
        {
            _configuration = configuration;
            _featureSwitchOptions = featureSwitchOptions;
        }

        public bool ForceApplicationInsightsTracking { get; set; }
        public bool ForceGoogleAnalyticsTracking { get; set; }
        public bool StickySessions { get; set; } = true;
        public bool DisableLockoutProtection { get; set; }
        public bool SimulateMessageSend { get; set; }
        public bool ShowPinInPost { get; set; }
        public bool SendPinByEmail { get; set; }
        public bool ShowEmailVerifyLink { get; set; }
        public bool LoadTesting { get; set; }
        public bool SearchCompaniesHouse { get; set; }

        public string TestPrefix { get; set; }

        public string WhitelistUsers { get; set; }

        #region Environment
        public string Environment => _configuration[HostDefaults.EnvironmentKey];

        public bool WhitelistingEnabled => !string.IsNullOrWhiteSpace(WhitelistUsers) && !_featureSwitchOptions.IsEnabled("LiveService");

        public bool IsWhitelistUser(string emailAddress)
        {
            if (!WhitelistingEnabled) return true;

            if (!emailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");

            return emailAddress.LikeAny(WhitelistUsers.SplitI(';'));
        }

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
                var serviceLive = _featureSwitchOptions.IsEnabled("LiveService");

                if (!string.IsNullOrWhiteSpace(TestPrefix)) exceptions.Add(new ConfigurationErrorsException("TestPrefix is not permitted in Production environment"));
                if (serviceLive)
                {
                    if (LoadTesting) exceptions.Add(new ConfigurationErrorsException("LoadTesting is not permitted in the live Production environment"));
                    if (SimulateMessageSend) exceptions.Add(new ConfigurationErrorsException("SimulateLetterSend is not permitted in the live Production environment"));
                    if (ShowPinInPost) exceptions.Add(new ConfigurationErrorsException("ShowPinInPost is not permitted in the live Production environment"));
                    if (SendPinByEmail) exceptions.Add(new ConfigurationErrorsException("SendPinByEmail is not permitted in the live Production environment"));
                    if (ShowEmailVerifyLink) exceptions.Add(new ConfigurationErrorsException("ShowEmailVerifyLink is not permitted in the live Production environment"));
                    if (DisableLockoutProtection) exceptions.Add(new ConfigurationErrorsException("DisableLockoutProtection is not permitted in the live Production environment"));
                }
            }
            else
            {
                if (LoadTesting)
                {
                    DisableLockoutProtection = true;
                    SimulateMessageSend = true;
                    ShowEmailVerifyLink = true;
                    ShowPinInPost = true;
                    if (string.IsNullOrWhiteSpace(TestPrefix)) exceptions.Add(new ConfigurationErrorsException("TestPrefix cannot be empty when LoadTesting=true"));
                }
            }
            if (ShowPinInPost && SendPinByEmail) exceptions.Add(new ConfigurationErrorsException("Cannot both show PIN onscreen and send PIN via email"));
            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1) throw exceptions[0];
                throw new AggregateException(exceptions);
            }
        }
    }
}