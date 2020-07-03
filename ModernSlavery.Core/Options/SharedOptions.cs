using System;
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
        public string ContentRoot { get; set; }
        public string WebRoot { get; set; }
        private string _DevelopmentWebroot;
        public string DevelopmentWebroot { get => _DevelopmentWebroot; set => _DevelopmentWebroot = value!=null && value.StartsWith('.') ? Path.GetFullPath(value) : value; }

        public int FirstReportingYear { get; set; } = 2020;
        public DateTime PrivateAccountingDate { get; set; }
        public DateTime PublicAccountingDate { get; set; }

        private string _IdentityIssuer;

        private int[] _reminderEmailDays;

        public string DefaultEncryptionKey { get; set; }

        public string AdminEmails { get; set; }
        public string SuperAdminEmails { get; set; }
        public string DatabaseAdminEmails { get; set; }

        public int SessionTimeOutMinutes { get; set; } = 20;

        public int ObfuscationSeed { get; set; } = 127;

        public int CertExpiresWarningDays { get; set; } = 30;

        public string TrustedIpDomains { get; set; }

        public bool UseDeveloperExceptions { get; set; }
        public string StartUrl { get; set; }
        public string DoneUrl { get; set; } = "https://www.gov.uk/";

        public bool EnableSubmitAlerts { get; set; } = true;

        public bool EncryptEmails { get; set; } = true;
        public bool MaintenanceMode { get; set; }
        public bool StickySessions { get; set; } = true;


        public DateTime PrivacyChangedDate { get; set; }
        public int EmailVerificationExpiryHours { get; set; }
        public int EmailVerificationMinResendHours { get; set; }
        public int EmployerCodeLength { get; set; }
        public int EmployerPageSize { get; set; }
        public string WEBSITE_HOSTNAME { get; set; }

        public string SiteAuthority => $"https://{WEBSITE_HOSTNAME}/";

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
        public string EmployerCodeChars { get; set; }
        public string PasswordRegex { get; set; }
        public string PasswordRegexError { get; set; }
        public string PinChars { get; set; }
        public string PinRegex { get; set; }
        public string PinRegexError { get; set; }
        public string TestPrefix { get; set; }
        public string WhoNeedsToReportGuidanceLink { get; set; }

        public string CompanyNumberRegexError { get; set; }
        public Version Version => Misc.GetTopAssembly().GetName().Version;
        public DateTime AssemblyDate => Misc.GetTopAssembly().GetAssemblyCreationTime();
        public string AssemblyCopyright => Misc.GetTopAssembly().GetAssemblyCopyright();
        public string DatabaseConnectionName { get; set; } = "ModernSlaveryDatabase";

        public string GpgReportingEmail { get; set; }
        public string DataControllerEmail { get; set; }
        public string DataProtectionOfficerEmail { get; set; }

        public string GovUkNotifyPinInThePostTemplateId { get; set; }

        public DateTime ActionHubSwitchOverDate { get; set; }

        public bool SendGoogleAnalyticsDataToGovUk { get; set; }

        public string Website_Instance_Id { get; set; }
        public string Website_Load_Certificates { get; set; }

        public string Website_Run_Rom_Package { get; set; }
        public string LocalAppData { get; set; }
        public string CertThumprint { get; set; }


        public bool SkipSpamProtection { get; set; }
        public int MaxNumCallsCompaniesHouseApiPerFiveMins { get; set; } = 500;

        public int[] ReminderEmailDays
        {
            get => _reminderEmailDays;
            set => _reminderEmailDays = value.OrderBy(d => d).ToArray();
        }

        public bool PinInPostTestMode { get; set; }
        public bool ShowEmailVerifyLink { get; set; }
        public string GoogleAnalyticsAccountId { get; set; }
        public string DateTimeOffset { get; set; }

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

        public string SaveDraftPath { get; set; }
        #endregion
    }
}