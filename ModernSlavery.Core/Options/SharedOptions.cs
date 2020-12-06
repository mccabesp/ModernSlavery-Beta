using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Core.Models
{
    [Options("root")]
    public class SharedOptions : IOptions
    {
        public SharedOptions()
        {

        }

        public string ApplicationName { get; set; }
        public string ServiceName { get; set; }
        private string _DevelopmentWebroot;
        public string DevelopmentWebroot { get => _DevelopmentWebroot; set => _DevelopmentWebroot = value != null && value.StartsWith('.') ? Path.GetFullPath(value) : value; }

        public int FirstReportingDeadlineYear { get; set; } = 2020;
        public DateTime PrivateReportingDeadline { get; set; }
        public DateTime PublicReportingDeadline { get; set; }

        private int[] _reminderEmailDays;

        public string DefaultEncryptionKey { get; set; }

        public string AdminEmails { get; set; }
        public string SuperAdminEmails { get; set; }
        public string DatabaseAdminEmails { get; set; }

        public int SessionTimeOutMinutes { get; set; } = 20;

        public int ObfuscationSeed { get; set; } = 127;

        public int CertExpiresWarningDays { get; set; } = 30;

        public string TrustedDomainsOrIPs { get; set; }
        private string[] _trustedDomainsOrIPs = null;

        public bool UseDeveloperExceptions { get; set; }

        public bool EnableSubmitAlerts { get; set; } = true;

        public bool MaintenanceMode { get; set; }

        public DateTime PrivacyChangedDate { get; set; }
        public int EmailVerificationExpiryHours { get; set; }
        public int EmailVerificationMinResendHours { get; set; }
        public int OrganisationCodeLength { get; set; }
        public int OrganisationPageSize { get; set; }
        public string EXTERNAL_HOSTNAME { get; set; }//The public internet host name
        public string WEBSITE_HOSTNAME { get; set; }//The AzureWebsites host name

        public int LevenshteinDistance { get; set; } = 5;
        public int LockoutMinutes { get; set; }
        public int MaxEmailVerifyAttempts { get; set; }
        public int MaxLoginAttempts { get; set; } = 3;
        public int MaxPinAttempts { get; set; } = 3;
        public int MaxSnapshotDays { get; set; } = 35;
        public int MinPasswordResetMinutes { get; set; } = 30;
        public int MinSignupMinutes { get; set; }
        public int PinInPostExpiryDays { get; set; }
        public DateTime PinExpiresDate => VirtualDateTime.Now.AddDays(0 - PinInPostExpiryDays);

        public int PinInPostMinRepostDays { get; set; }
        public int PinLength { get; set; }
        public int PurgeRetiredReturnDays { get; set; } = 30;
        public int PurgeUnusedOrganisationDays { get; set; } = 30;
        public int PurgeUnverifiedUserDays { get; set; } = 7;
        public int PurgeUnconfirmedPinDays { get; set; } = 14;
        public string SecurityCodeChars { get; set; } = "123456789ABCDEFGHKLMNPQRSTUXYZ";
        public int SecurityCodeLength { get; set; } = 8;
        public int SecurityCodeExpiryDays { get; set; } = 90;
        public string OrganisationCodeChars { get; set; }
        public string PasswordRegex { get; set; }
        public string PasswordRegexError { get; set; }
        public string PinChars { get; set; }
        public string PinRegex { get; set; }
        public string PinRegexError { get; set; }
        public string WhoNeedsToReportGuidanceLink { get; set; }

        public Version Version => Misc.GetTopAssembly().GetName().Version;
        public DateTime AssemblyDate => Misc.GetTopAssembly().GetAssemblyCreationTime();
        public string AssemblyCopyright => Misc.GetTopAssembly().GetAssemblyCopyright();
        public string DatabaseConnectionName { get; set; } = "ModernSlaveryDatabase";

        public string MsuReportingEmail { get; set; }
        public string DataControllerEmail { get; set; }
        public string DataProtectionOfficerEmail { get; set; }

        public string GovUkNotifyPinInThePostTemplateId { get; set; }

        public DateTime ActionHubSwitchOverDate { get; set; }

        public bool SendGoogleAnalyticsDataToGovUk { get; set; }

        public string Website_Instance_Id { get; set; }

        public string CertThumbprint { get; set; }
        public string CertFilepath { get; set; }
        public string CertPassword { get; set; }

        public int[] ReminderEmailDays
        {
            get => _reminderEmailDays;
            set => _reminderEmailDays = value.OrderBy(d => d).ToArray();
        }

        public string GoogleAnalyticsAccountId { get; set; }

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        #region Environment

        public string Environment { get; set; }

        public bool IsEnvironment(params string[] environmentNames)
        {
            return environmentNames.Any(en => Environment.EqualsI(en));
        }

        public bool IsDevelopment()
        {
            return IsEnvironment("Development");
        }
        public bool IsTest()
        {
            return IsEnvironment("Test");
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

        #region Files and Directories

        public string DataPath { get; set; }
        public string DownloadsPath => Path.Combine(DataPath, "Downloads");

        public string LogPath { get; set; }

        public string DownloadsLocation { get; set; }

        #endregion

        public void Validate()
        {
            var exceptions = new List<Exception>();
            //Check security settings for production environment
            if (IsProduction())
            {
                if (string.IsNullOrWhiteSpace(TrustedDomainsOrIPs)) exceptions.Add(new ConfigurationErrorsException($"{nameof(TrustedDomainsOrIPs)} cannot be empty in Production environment"));
                if (string.IsNullOrWhiteSpace(DefaultEncryptionKey)) exceptions.Add(new ConfigurationErrorsException($"{nameof(DefaultEncryptionKey)} cannot be empty in Production environment"));
                if (DefaultEncryptionKey == Encryption.DefaultEncryptionKey) exceptions.Add(new ConfigurationErrorsException($"{nameof(DefaultEncryptionKey)} cannot use default value in Production environment"));
                if (ObfuscationSeed.IsAny(0, 127)) exceptions.Add(new ConfigurationErrorsException($"{nameof(ObfuscationSeed)} cannot use default value in Production environment"));
                if (string.IsNullOrWhiteSpace(CertThumbprint)) exceptions.Add(new ConfigurationErrorsException($"{nameof(CertThumbprint)} cannot be empty in Production environment."));
            }

            if (string.IsNullOrWhiteSpace(AdminEmails))
                exceptions.Add(new ConfigurationErrorsException($"Missing {nameof(AdminEmails)}"));
            if (string.IsNullOrWhiteSpace(DatabaseAdminEmails))
                exceptions.Add(new ConfigurationErrorsException($"Missing {nameof(DatabaseAdminEmails)}"));
            if (string.IsNullOrWhiteSpace(SuperAdminEmails))
                exceptions.Add(new ConfigurationErrorsException($"Missing {nameof(SuperAdminEmails)}"));

            if (string.IsNullOrWhiteSpace(ApplicationName)) exceptions.Add(new ConfigurationErrorsException($"Missing {nameof(ApplicationName)}"));
            if (string.IsNullOrWhiteSpace(CertFilepath) && !string.IsNullOrWhiteSpace(CertPassword)) exceptions.Add(new ConfigurationErrorsException($"Missing {nameof(CertFilepath)}"));
            if (!string.IsNullOrWhiteSpace(CertFilepath) && string.IsNullOrWhiteSpace(CertPassword)) exceptions.Add(new ConfigurationErrorsException($"Missing {nameof(CertPassword)}"));

            if (FirstReportingDeadlineYear == 0 || FirstReportingDeadlineYear > VirtualDateTime.Now.Year) exceptions.Add(new ConfigurationErrorsException($"Invalid {nameof(FirstReportingDeadlineYear)}: {FirstReportingDeadlineYear}."));
            if (PrivateReportingDeadline == DateTime.MinValue)
                exceptions.Add(new ConfigurationErrorsException($"Invalid {nameof(PrivateReportingDeadline)}: {PrivateReportingDeadline}."));
            else
                while (PrivateReportingDeadline.Date.AddDays(1) < VirtualDateTime.Now)
                    PrivateReportingDeadline = new DateTime(PrivateReportingDeadline.Year + 1, PrivateReportingDeadline.Month, PrivateReportingDeadline.Day);

            if (PublicReportingDeadline == DateTime.MinValue)
                exceptions.Add(new ConfigurationErrorsException($"Invalid {nameof(PublicReportingDeadline)}: {PublicReportingDeadline}."));
            else
                while (PublicReportingDeadline.Date.AddDays(1) < VirtualDateTime.Now)
                    PublicReportingDeadline = new DateTime(PublicReportingDeadline.Year + 1, PublicReportingDeadline.Month, PublicReportingDeadline.Day);

            if (string.IsNullOrWhiteSpace(DataPath)) throw new ConfigurationErrorsException($"{nameof(DataPath)} cannot be empty");

            if (!string.IsNullOrWhiteSpace(TrustedDomainsOrIPs))
            {
                _trustedDomainsOrIPs = TrustedDomainsOrIPs.SplitI();
                if (_trustedDomainsOrIPs == null || _trustedDomainsOrIPs.Length == 0)
                    throw new ConfigurationErrorsException($"{nameof(TrustedDomainsOrIPs)} cannot be empty");
            }

            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1) throw exceptions[0];
                throw new AggregateException(exceptions);
            }


        }

        public bool IsTrustedAddress(string testIPAddress)
        {
            if (_trustedDomainsOrIPs == null) return true;
            if (string.IsNullOrWhiteSpace(testIPAddress)) throw new ArgumentNullException(nameof(testIPAddress));
            return Networking.IsTrustedAddress(testIPAddress, _trustedDomainsOrIPs);
        }

    }
}