using System.ComponentModel.DataAnnotations;

namespace ModernSlavery.Core.Entities
{
    public enum SectorTypes: byte
    {
        Unknown = 0,
        Private = 1,
        Public = 2
    }
    public enum UserStatuses : byte
    {
        Unknown = 0,
        New = 1,
        Suspended = 2,
        Active = 3,
        Retired = 4
    }

    public enum UserSettingKeys : byte
    {
        Unknown = 0,
        SendUpdates = 1,
        AllowContact = 2,
        PendingFasttrackCodes = 3,
        AcceptedPrivacyStatement = 4
    }

    public enum OrganisationStatuses : byte
    {
        Unknown = 0,
        New = 1,
        Suspended = 2,
        Active = 3,
        Retired = 4,
        Pending = 5,
        Deleted = 6
    }

    public enum AddressStatuses : byte
    {
        Unknown = 0,
        New = 1,
        Suspended = 2,
        Active = 3,
        Pending = 5,
        Retired = 6
    }

    public enum RegistrationMethods: byte
    {
        Unknown = 0,
        PinInPost = 1,
        EmailDomain = 2,
        Manual = 3,
        Fasttrack = 4
    }

    public enum StatementStatuses : byte
    {
        Unknown = 0,
        Draft = 1,
        Suspended = 2,
        Submitted = 3,
        Retired = 4,
        Deleted = 5
    }

    public enum StatementTurnovers : byte
    {
        //Not Provided
        [Range(0, 0)]
        NotProvided = 0,

        //Under £36 million
        [Display(Description = "Under £36 million")]
        [Range(0, 36)]
        Under36Million = 1,

        //£36 million - £60 million
        [Display(Description = "£36 million to £60 million")]
        [Range(36, 60)]
        From36to60Million = 2,

        //£60 million - £100 million
        [Display(Description = "£60 million to £100 million")]
        [Range(60, 100)]
        From60to100Million = 3,

        //£100 million - £500 million
        [Display(Description = "£100 million to £500 million")]
        [Range(100, 500)]
        From100to500Million = 4,

        //£500 million+
        [Display(Description = "Over £500 million")]
        [Range(500, 0)]
        Over500Million = 5,
    }

    public enum StatementYears : byte
    {
        //Not Provided
        [Range(0, 0)]
        NotProvided = 0,

        //This is the first time
        [Display(Description = "This is the first time")]
        [Range(1, 1)]
        Year1 = 1,

        //1 to 5 Years
        [Display(Description = "1 to 5 years")]
        [Range(1, 5)]
        Years1To5 = 2,

        //More than 5 years
        [Display(Description = "More than 5 years")]
        [Range(5, 0)]
        Over5Years = 3,
    }

    public enum ScopeRowStatuses : byte
    {
        Unknown = 0,
        Active = 3,
        Retired = 4
    }

    public enum ScopeStatuses: byte
    {
        [Display(Name = "In scope")] Unknown = 0,

        [Display(Name = "In scope")] InScope = 1,

        [Display(Name = "Out of scope")] OutOfScope = 2,

        [Display(Name = "In scope")] PresumedInScope = 3,

        [Display(Name = "Out of scope")] PresumedOutOfScope = 4
    }

    public enum RegisterStatuses: byte
    {
        Unknown = 0,
        RegisterSkipped = 1,
        RegisterPending = 2,
        RegisterComplete = 3,
        RegisterCancelled = 4
    }

    public enum AuditedAction:byte
    {
        [Display(Name = "Change late flag")] AdminChangeLateFlag = 0,

        [Display(Name = "Change organisation scope")]
        AdminChangeOrganisationScope = 1,

        [Display(Name = "Change companies house opting")]
        AdminChangeCompaniesHouseOpting = 2,

        [Display(Name = "Change organisation name")]
        AdminChangeOrganisationName = 3,

        [Display(Name = "Change organisation address")]
        AdminChangeOrganisationAddress = 4,

        [Display(Name = "Change organisation SIC code")]
        AdminChangeOrganisationSicCode = 5,

        [Display(Name = "Change user contact preferences")]
        AdminChangeUserContactPreferences = 6,

        [Display(Name = "Re-send verification email")]
        AdminResendVerificationEmail = 7,

        [Display(Name = "Change organisation public sector classification")]
        AdminChangeOrganisationPublicSectorClassification = 8
    }

    public enum HashingAlgorithm
    {
        Unhashed = -1,
        Unknown = 0,
        SHA512 = 1,
        PBKDF2 = 2,
        PBKDF2AppliedToSHA512 = 3
    }
}