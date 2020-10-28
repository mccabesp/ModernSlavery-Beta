using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core
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
        public const string Organisations = "MSU-Organisations.csv";
        public const string Users = "MSU-Users.csv";
        public const string Registrations = "MSU-Registrations.csv";
        public const string RegistrationAddresses = "MSU-RegistrationAddresses.csv";
        public const string UnverifiedRegistrations = "MSU-UnverifiedRegistrations.csv";
        public const string SendInfo = "MSU-UsersToSendInfo.csv";
        public const string AllowFeedback = "MSU-UsersToContactForFeedback.csv";
        public const string UnfinishedOrganisations = "MSU-UnfinishedOrgs.csv";
        public const string OrphanOrganisations = "MSU-OrphanOrganisations.csv";
        public const string OrganisationScopes = "MSU-Scopes.csv";
        public const string OrganisationSubmissions = "MSU-Submissions.csv";
        public const string ShortCodes = "MSU-ShortCodes.csv";
        public const string SicCodes = "MSU-SicCodes.csv";
        public const string SicSections = "MSU-SicSections.csv";
        public const string SicSectorSynonyms = "MSU-SicSectorSynonyms.csv";

        public const string StatementSectorTypes = "MSU-StatementSectorTypes.csv";

        // Record logs
        public const string BadSicLog = "MSU-BadSicLog.csv";
        public const string ManualChangeLog = "MSU-ManualChangeLog.csv";
        public const string RegistrationLog = "MSU-RegistrationLog.csv";
        public const string SubmissionLog = "MSU-SubmissionLog.csv";
        public const string EmailSendLog = "MSU-EmailSendLog.csv";
        public const string StannpSendLog = "MSU-StannpSendLog.csv";
        public const string SearchLog = "MSU-searchLog.csv";
        public const string UserLog = "MSU-UserLog.csv";
        public const string ImportPrivateOrganisations = "MSU-ImportPrivateOrganisations.csv";
        public const string ImportPublicOrganisations = "MSU-ImportPublicOrganisations.csv";

        public static string GetRootFilename(string filePath)
        {
            var path = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            var prefix = fileName.BeforeFirst("_");
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