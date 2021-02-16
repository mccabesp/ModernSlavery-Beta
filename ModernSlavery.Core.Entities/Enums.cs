using System;
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