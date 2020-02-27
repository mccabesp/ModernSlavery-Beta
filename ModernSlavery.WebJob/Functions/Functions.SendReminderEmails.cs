using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Classes.Logger;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Entities;
using ModernSlavery.Entities;
using ModernSlavery.Extensions;
using ModernSlavery.Extensions.AspNetCore;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using ModernSlavery.Entities.Enums;
using ModernSlavery.SharedKernel;

namespace ModernSlavery.WebJob
{
    public partial class Functions
    {

        // This trigger is set to run every hour, on the hour
        public void SendReminderEmails([TimerTrigger("0 * * * *")] TimerInfo timer)
        {
            DateTime start = VirtualDateTime.Now;
            CustomLogger.Information("SendReminderEmails Function started", start);
            
            List<int> reminderDays = GetReminderEmailDays();
            if (reminderDays.Count == 0)
            {
                CustomLogger.Information("SendReminderEmails Function finished. No ReminderEmailDays set.");
                return;
            }

            IEnumerable<User> users = _DataRepository.GetAll<User>();

            foreach (User user in users)
            {
                if (VirtualDateTime.Now > start.AddMinutes(59))
                {
                    CustomLogger.Information("Hit timeout break");
                    break;
                }

                List<Organisation> inScopeOrganisationsThatStillNeedToReport = user.UserOrganisations
                    .Select(uo => uo.Organisation)
                    .Where(
                        o =>
                            o.LatestScope.ScopeStatus == ScopeStatuses.InScope
                            || o.LatestScope.ScopeStatus == ScopeStatuses.PresumedInScope)
                    .Where(
                        o =>
                            o.LatestReturn == null
                            || o.LatestReturn.AccountingDate != _snapshotDateHelper.GetSnapshotDate(o.SectorType)
                            || o.LatestReturn.Status != ReturnStatuses.Submitted)
                    .ToList();

                if (inScopeOrganisationsThatStillNeedToReport.Count > 0)
                {
                    SendReminderEmailsForSectorType(user, inScopeOrganisationsThatStillNeedToReport, SectorTypes.Public);
                    SendReminderEmailsForSectorType(user, inScopeOrganisationsThatStillNeedToReport, SectorTypes.Private);
                }
            }
            
            CustomLogger.Information("SendReminderEmails Function finished");
        }

        private void SendReminderEmailsForSectorType(
            User user,
            List<Organisation> inScopeOrganisationsThatStillNeedToReport,
            SectorTypes sectorType)
        {
            List<Organisation> organisationsOfSectorType = inScopeOrganisationsThatStillNeedToReport
                .Where(o => o.SectorType == sectorType)
                .ToList();

            if (organisationsOfSectorType.Count > 0)
            {
                if (IsAfterEarliestReminder(sectorType)
                    && ReminderEmailWasNotSentAfterLatestReminderDate(user, sectorType))
                {
                    try
                    {
                        SendReminderEmail(user, sectorType, organisationsOfSectorType);
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.Error(
                            "Failed whilst sending or saving reminder email",
                            new
                            {
                                UserId = user.UserId,
                                SectorType = sectorType,
                                OrganisationIds = inScopeOrganisationsThatStillNeedToReport.Select(o => o.OrganisationId),
                                Exception = ex.Message
                            });
                    }
                }
            }
        }

        private void SendReminderEmail(User user,
            SectorTypes sectorType,
            List<Organisation> organisations)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"DeadlineDate", _snapshotDateHelper.GetSnapshotDate(sectorType).AddYears(1).AddDays(-1).ToString("d MMMM yyyy")},
                {"DaysUntilDeadline", _snapshotDateHelper.GetSnapshotDate(sectorType).AddYears(1).AddDays(-1).Subtract(VirtualDateTime.Now).Days},
                {"OrganisationNames", GetOrganisationNameString(organisations)},
                {"OrganisationIsSingular", organisations.Count == 1},
                {"OrganisationIsPlural", organisations.Count > 1},
                {"SectorType", sectorType.ToString().ToLower()},
                {"Environment", Config.IsProduction() ? "" : $"[{Config.EnvironmentName}] "}
            };

            var notifyEmail = new NotifyEmail
            {
                EmailAddress = user.EmailAddress, TemplateId = "db15432c-9eda-4df4-ac67-290c7232c546", Personalisation = personalisation
            };
            
            govNotifyApi.SendEmail(notifyEmail);
            SaveReminderEmailRecord(user, sectorType);
        }
        
        private static void SaveReminderEmailRecord(User user, SectorTypes sectorType)
        {
            var reminderEmailRecord = new ReminderEmail {UserId = user.UserId, SectorType = sectorType, DateSent = VirtualDateTime.Now};
            var dataRepository = Program.ContainerIOC.Resolve<IDataRepository>();
            dataRepository.Insert(reminderEmailRecord);
            dataRepository.SaveChangesAsync().Wait();
        }

        private string GetOrganisationNameString(List<Organisation> organisations)
        {
            if (organisations.Count == 1)
            {
                return organisations[0].OrganisationName;
            }

            if (organisations.Count == 2)
            {
                return $"{organisations[0].OrganisationName} and {organisations[1].OrganisationName}";
            }

            return $"{organisations[0].OrganisationName} and {organisations.Count - 1} other organisations";
        }

        private bool IsAfterEarliestReminder(SectorTypes sectorType)
        {
            return VirtualDateTime.Now > GetEarliestReminderDate(sectorType);
        }

        private bool ReminderEmailWasNotSentAfterLatestReminderDate(User user, SectorTypes sectorType)
        {
            ReminderEmail latestReminderEmail = _DataRepository.GetAll<ReminderEmail>()
                .Where(re => re.UserId == user.UserId)
                .Where(re => re.SectorType == sectorType)
                .OrderByDescending(re => re.DateSent)
                .FirstOrDefault();

            if (latestReminderEmail == null)
            {
                return true;
            }

            DateTime latestReminderEmailDate = GetLatestReminderEmailDate(sectorType);
            return latestReminderEmail.DateSent <= latestReminderEmailDate;
        }

        private DateTime GetEarliestReminderDate(SectorTypes sectorType)
        {
            List<int> reminderEmailDays = GetReminderEmailDays();
            int earliestReminderDay = reminderEmailDays[reminderEmailDays.Count - 1];

            DateTime deadlineDate = GetDeadlineDate(sectorType);
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
            List<int> reminderDays = GetReminderEmailDays();
            DateTime deadlineDate = GetDeadlineDate(sectorType);

            return reminderDays.Select(reminderDay => deadlineDate.AddDays(-reminderDay)).ToList();
        }

        private DateTime GetDeadlineDate(SectorTypes sectorType)
        {
            return _snapshotDateHelper.GetSnapshotDate(sectorType).AddYears(1);
        }

        private static List<int> GetReminderEmailDays()
        {
            var reminderEmailDays = JsonConvert.DeserializeObject<List<int>>(Config.GetAppSetting("ReminderEmailDays"));
            reminderEmailDays.Sort();
            return reminderEmailDays;
        }

    }
}
