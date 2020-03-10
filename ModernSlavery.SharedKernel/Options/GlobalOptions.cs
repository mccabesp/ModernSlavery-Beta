﻿using System;
using System.IO;
using ModernSlavery.Extensions;

namespace ModernSlavery.SharedKernel.Options
{
    public class GlobalOptions
    {
        public int SessionTimeOutMinutes { get; set; } = 20;

        #region Files and Directories
        public string DataPath { get; set; }
        public string DownloadsPath => Path.Combine(DataPath, "Downloads");

        public string LogPath { get; set; }

        public string DownloadsLocation { get; set; }

        public string SaveDraftPath { get; set; }

        #endregion


        public int CertExpiresWarningDays { get; set; }=30;

        public string TrustedIPDomains { get; set; }

        public bool UseDeveloperExceptions { get; set; }
        public string StartUrl { get; set; }
        public string DoneUrl { get; set; }

        public bool EnableSubmitAlerts { get; set; } = true;

        public bool EncryptEmails { get; set; }=true;
        public bool MaintenanceMode { get; set; }
        public bool StickySessions { get; set; }=true;


        public DateTime PrivacyChangedDate { get; set; }
        public int EmailVerificationExpiryHours { get; set; }
        public int EmailVerificationMinResendHours { get; set; }
        public int EmployerCodeLength { get; set; }
        public int EmployerPageSize { get; set; }
        public string EXTERNAL_HOST { get; set; }
        public int LevenshteinDistance { get; set; } = 5;
        public int LockoutMinutes { get; set; }
        public int MaxEmailVerifyAttempts { get; set; }
        public int MaxLoginAttempts { get; set; } = 5;
        public int MaxPinAttempts { get; set; } = 5;
        public int MinPasswordResetMinutes { get; set; }=30;
        public int MinSignupMinutes { get; set; }
        public int PinInPostExpiryDays { get; set; }
        public DateTime PinExpiresDate => VirtualDateTime.Now.AddDays(0 - PinInPostExpiryDays);

        public int PinInPostMinRepostDays { get; set; }
        public int PINLength { get; set; }
        public int PurgeRetiredReturnDays { get; set; } = 30;
        public int PurgeUnusedOrganisationDays { get; set; } = 30;
        public int PurgeUnverifiedUserDays { get; set; } = 7;
        public int PurgeUnconfirmedPinDays { get; set; } = 14;
        public int SecurityCodeExpiryDays { get; set; } = 90;
        public bool DisablePageCaching { get; set; }

        public string EmployerCodeChars { get; set; }
        public string PasswordRegex { get; set; }
        public string PasswordRegexError { get; set; }
        public string PINChars { get; set; }
        public string PinRegex { get; set; }
        public string PinRegexError { get; set; }
        public string TestPrefix { get; set; }
        public string WhoNeedsToReportGuidanceLink { get; set; }

        public string CompanyNumberRegexError { get; set; }
        public Version Version => Misc.GetTopAssembly().GetName().Version;
        public DateTime AssemblyDate => Misc.GetTopAssembly().GetAssemblyCreationTime();
        public string AssemblyCopyright => Misc.GetTopAssembly().GetAssemblyCopyright();
        public string DatabaseConnectionName { get; set; } = "ModernSlaveryDatabase";

        public int FirstReportingYear { get; set; } = 2020;

        public string GpgReportingEmail { get; set; }
        public string DataControllerEmail { get; set; }
        public string DataProtectionOfficerEmail { get; set; }

        public string GovUkNotifyPinInThePostTemplateId { get; set; }

        public DateTime ActionHubSwitchOverDate { get; set; }
        
        public bool SendGoogleAnalyticsDataToGovUk { get; set; }

        public string WEBSITE_INSTANCE_ID { get; set; }
    }
}
