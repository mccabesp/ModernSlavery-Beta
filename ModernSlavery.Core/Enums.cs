using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ModernSlavery.Core
{

    public enum ManualActions : byte
    {
        Unknown = 0,
        Create = 1,
        Read = 2,
        Update = 3,
        Delete = 4,
        Extend = 5,
        Expire = 6
    }

    public enum UserRoleTypes
    {
        Unknown,
        Anonymous,
        Submitter,
        BasicAdmin,
        DatabaseAdmin,
        SuperAdmin,
        DevOpsAdmin
    }

    public enum UserAction : byte
    {
        CreatedAccount = 0,
        ChangedEmail = 1,
        ChangedPassword = 2,
        ChangedDetails = 3,
        Retired = 4
    }

    public enum SearchModes : byte
    {
        [EnumMember(Value = "any")] Any,
        [EnumMember(Value = "all")] All
    }

    public enum SearchReportingStatusFilter : byte
    {
        [Display(Name = "Reported in the last 7 days")]
        ReportedInTheLast7Days,

        [Display(Name = "Reported in the last 30 days")]
        ReportedInTheLast30Days,

        [Display(Name = "Reported late")] ReportedLate,

        [Display(Name = "Reported with extra information")]
        ExplanationProvidedByOrganisation
    }

    public enum StatementErrors : byte
    {
        Unknown = 0,
        NotFound = 1,
        Unauthorised = 2,
        Locked = 3,
        TooLate = 4,
        DuplicateName = 5,
        CoHoTransientError = 6,
        CoHoPermanentError = 7
    }

    public enum RetryPolicyTypes
    {
        None,
        Linear,
        Exponential
    }
}