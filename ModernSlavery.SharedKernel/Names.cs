using ModernSlavery.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ModernSlavery.SharedKernel
{
    public static class EnumHelper
    {
        public static string DisplayNameOf(object obj)
        {
            return obj.GetType()
                ?
                .GetMember(obj.ToString())
                ?.First()
                ?
                .GetCustomAttribute<DisplayAttribute>()
                ?.Name;
        }

    }

    public static class Filenames
    {

        public const string Organisations = "GPG-Organisations.csv";
        public const string Users = "GPG-Users.csv";
        public const string Registrations = "GPG-Registrations.csv";
        public const string RegistrationAddresses = "GPG-RegistrationAddresses.csv";
        public const string UnverifiedRegistrations = "GPG-UnverifiedRegistrations.csv";
        public const string SendInfo = "GPG-UsersToSendInfo.csv";
        public const string AllowFeedback = "GPG-UsersToContactForFeedback.csv";
        public const string UnfinishedOrganisations = "GPG-UnfinishedOrgs.csv";
        public const string OrphanOrganisations = "GPG-OrphanOrganisations.csv";
        public const string OrganisationScopes = "GPG-Scopes.csv";
        public const string OrganisationSubmissions = "GPG-Submissions.csv";
        public const string OrganisationLateSubmissions = "GPG-LateSubmissions.csv";
        public const string ShortCodes = "GPG-ShortCodes.csv";
        public const string SicCodes = "SicCodes.csv";
        public const string SicSections = "SicSections.csv";
        public const string SicSectorSynonyms = "GPG-SicSectorSynonyms.csv";

        // Record logs
        public const string BadSicLog = "BadSicLog.csv";
        public const string ManualChangeLog = "ManualChangeLog.csv";
        public const string RegistrationLog = "RegistrationLog.csv";
        public const string SubmissionLog = "SubmissionLog.csv";
        public const string EmailSendLog = "EmailSendLog.csv";
        public const string StannpSendLog = "StannpSendLog.csv";
        public const string SearchLog = "searchLog.csv";
        public const string UserLog = "UserLog.csv";

        public static string DnBOrganisations(int year)=>$"GPG-DnBOrgs_{year:yyyy}-{year++:yy}.csv";

        public static string PreviousDnBOrganisations(int year)=>$"GPG-DnBOrgs_{year--:yyyy}-{year:yy}.csv";

        public static string GetRootFilename(string filePath)
        {
            string path = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            string prefix = fileName.BeforeFirst("_");
            return $"{prefix}{extension}";
        }

    }

    public static class QueueNames
    {

        public const string ExecuteWebJob = "execute-webjob";
        public const string SendEmail = "send-email";
        public const string SendNotifyEmail = "send-notify-email";
        public const string LogEvent = "log-event";
        public const string LogRecord = "log-record";

    }

    public static class CookieNames
    {

        public const string LastCompareQuery = "compare";

    }
}
