using System;
using System.ComponentModel.DataAnnotations;

namespace ModernSlavery.SharedKernel
{
    public enum SectorTypes
    {

        Unknown = 0,
        Private = 1,
        Public = 2

    }

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

    public enum UserAction : byte
    {

        CreatedAccount = 0,
        ChangedEmail = 1,
        ChangedPassword = 2,
        ChangedDetails = 3,
        Retired = 4

    }

    public enum SearchType
    {

        ByEmployerName = 1,
        BySectorType = 2,
        NotSet = 99

    }

    public enum SearchReportingStatusFilter
    {

        [Display(Name = "Reported in the last 7 days")]
        ReportedInTheLast7Days,

        [Display(Name = "Reported in the last 30 days")]
        ReportedInTheLast30Days,

        [Display(Name = "Reported late")]
        ReportedLate,

        [Display(Name = "Reported with extra information")]
        ExplanationProvidedByEmployer

    }

}
