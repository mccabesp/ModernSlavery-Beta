using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.Azure.WebJobs;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        // This trigger is set to run every hour, on the hour
        [Disable(typeof(DisableWebjobProvider))]
        public void SendReminderEmails([TimerTrigger("%SendReminderEmails%")] TimerInfo timer)
        {
            var start = VirtualDateTime.Now;
            _CustomLogger.Information("SendReminderEmails Function started", start);

            if (_SharedBusinessLogic.SharedOptions.ReminderEmailDays == null ||
                _SharedBusinessLogic.SharedOptions.ReminderEmailDays.Length == 0)
            {
                _CustomLogger.Information("SendReminderEmails Function finished. No ReminderEmailDays set.");
                return;
            }

            IEnumerable<User> users = _SharedBusinessLogic.DataRepository.GetAll<User>();

            foreach (var user in users)
            {
                if (VirtualDateTime.Now > start.AddMinutes(59))
                {
                    _CustomLogger.Information("Hit timeout break");
                    break;
                }

                var inScopeOrganisationsThatStillNeedToReport = user.UserOrganisations
                    .Select(uo => uo.Organisation)
                    .Where(
                        o =>
                            o.LatestScope.ScopeStatus == ScopeStatuses.InScope
                            || o.LatestScope.ScopeStatus == ScopeStatuses.PresumedInScope)
                    .Where(
                        o =>
                            o.LatestStatement == null
                            || o.LatestStatement.SubmissionDeadline != _snapshotDateHelper.GetReportingDeadline(o.SectorType)
                            || o.LatestStatement.Status != StatementStatuses.Submitted)
                    .ToList();

                if (inScopeOrganisationsThatStillNeedToReport.Count > 0)
                {
                    SendReminderEmailsForSectorType(user, inScopeOrganisationsThatStillNeedToReport,
                        SectorTypes.Public);
                    SendReminderEmailsForSectorType(user, inScopeOrganisationsThatStillNeedToReport,
                        SectorTypes.Private);
                }
            }

            _CustomLogger.Information("SendReminderEmails Function finished");
        }

        private void SendReminderEmailsForSectorType(
            User user,
            List<Organisation> inScopeOrganisationsThatStillNeedToReport,
            SectorTypes sectorType)
        {
            var organisationsOfSectorType = inScopeOrganisationsThatStillNeedToReport
                .Where(o => o.SectorType == sectorType)
                .ToList();

            if (organisationsOfSectorType.Count > 0)
                if (IsAfterEarliestReminder(sectorType)
                    && ReminderEmailWasNotSentAfterLatestReminderDate(user, sectorType))
                    try
                    {
                        SendReminderEmail(user, sectorType, organisationsOfSectorType);
                    }
                    catch (Exception ex)
                    {
                        _CustomLogger.Error(
                            "Failed whilst sending or saving reminder email",
                            new
                            {
                                user.UserId,
                                SectorType = sectorType,
                                OrganisationIds =
                                    inScopeOrganisationsThatStillNeedToReport.Select(o => o.OrganisationId),
                                Exception = ex.Message
                            });
                    }
        }

        private void SendReminderEmail(User user,
            SectorTypes sectorType,
            List<Organisation> organisations)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {
                    "DeadlineDate",
                    _snapshotDateHelper.GetReportingStartDate(sectorType).AddYears(1).AddDays(-1).ToString("d MMMM yyyy")
                },
                {
                    "DaysUntilDeadline",
                    _snapshotDateHelper.GetReportingStartDate(sectorType).AddYears(1).AddDays(-1)
                        .Subtract(VirtualDateTime.Now).Days
                },
                {"OrganisationNames", GetOrganisationNameString(organisations)},
                {"OrganisationIsSingular", organisations.Count == 1},
                {"OrganisationIsPlural", organisations.Count > 1},
                {"SectorType", sectorType.ToString().ToLower()},
                {
                    "Environment",
                    _SharedBusinessLogic.SharedOptions.IsProduction()
                        ? ""
                        : $"[{_SharedBusinessLogic.SharedOptions.Environment}] "
                }
            };

            var notifyEmail = new SendEmailRequest
            {
                EmailAddress = user.EmailAddress, TemplateId = "db15432c-9eda-4df4-ac67-290c7232c546",
                Personalisation = personalisation
            };

            govNotifyApi.SendEmail(notifyEmail);
            SaveReminderEmailRecord(user, sectorType);
        }

        private void SaveReminderEmailRecord(User user, SectorTypes sectorType)
        {
            var reminderEmailRecord = new ReminderEmail
                {UserId = user.UserId, SectorType = sectorType, DateSent = VirtualDateTime.Now};
            _SharedBusinessLogic.DataRepository.Insert(reminderEmailRecord);
            _SharedBusinessLogic.DataRepository.SaveChangesAsync().Wait();
        }

        private string GetOrganisationNameString(List<Organisation> organisations)
        {
            if (organisations.Count == 1) return organisations[0].OrganisationName;

            if (organisations.Count == 2)
                return $"{organisations[0].OrganisationName} and {organisations[1].OrganisationName}";

            return $"{organisations[0].OrganisationName} and {organisations.Count - 1} other organisations";
        }

        private bool IsAfterEarliestReminder(SectorTypes sectorType)
        {
            return VirtualDateTime.Now > GetEarliestReminderDate(sectorType);
        }

        private bool ReminderEmailWasNotSentAfterLatestReminderDate(User user, SectorTypes sectorType)
        {
            var latestReminderEmail = _SharedBusinessLogic.DataRepository.GetAll<ReminderEmail>()
                .Where(re => re.UserId == user.UserId)
                .Where(re => re.SectorType == sectorType)
                .OrderByDescending(re => re.DateSent)
                .FirstOrDefault();

            if (latestReminderEmail == null) return true;

            var latestReminderEmailDate = GetLatestReminderEmailDate(sectorType);
            return latestReminderEmail.DateSent <= latestReminderEmailDate;
        }

        private DateTime GetEarliestReminderDate(SectorTypes sectorType)
        {
            var earliestReminderDay =
                _SharedBusinessLogic.SharedOptions.ReminderEmailDays[
                    _SharedBusinessLogic.SharedOptions.ReminderEmailDays.Length - 1];

            var deadlineDate = GetDeadlineDate(sectorType);
            return deadlineDate.AddDays(-earliestReminderDay);
        }

        private DateTime GetLatestReminderEmailDate(SectorTypes sectorType)
        {
            return GetReminderDates(sectorType)
                .Where(reminderDate => reminderDate < VirtualDateTime.Now)
                .OrderBy(reminderDate => reminderDate)
                .FirstOrDefault();
        }

        private List<DateTime> GetReminderDates(SectorTypes sectorType)
        {
            var deadlineDate = GetDeadlineDate(sectorType);

            return _SharedBusinessLogic.SharedOptions.ReminderEmailDays
                .Select(reminderDay => deadlineDate.AddDays(-reminderDay)).ToList();
        }

        private DateTime GetDeadlineDate(SectorTypes sectorType)
        {
            return _snapshotDateHelper.GetReportingStartDate(sectorType).AddYears(1);
        }
    }
}