using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.Core;
using ModernSlavery.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.SharedKernel;
using ModernSlavery.Entities.Enums;

namespace ModernSlavery.WebJob
{
    public partial class Functions
    {

        public async Task UpdateUsersToContactForFeedback([TimerTrigger(typeof(EveryWorkingHourSchedule), RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                string filePath = Path.Combine(Global.DownloadsPath, Filenames.AllowFeedback);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateUsersToContactForFeedback))
                    && await FileRepository.GetFileExistsAsync(filePath))
                {
                    return;
                }

                await UpdateUsersToContactForFeedbackAsync(filePath);
                log.LogDebug($"Executed {nameof(UpdateUsersToContactForFeedback)}:successfully");
            }
            catch (Exception ex)
            {
                string message = $"Failed {nameof(UpdateUsersToContactForFeedback)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
            finally
            {
                StartedJobs.Add(nameof(UpdateUsersToContactForFeedback));
            }
        }

        public async Task UpdateUsersToContactForFeedbackAsync(string filePath)
        {
            if (RunningJobs.Contains(nameof(UpdateUsersToContactForFeedback)))
            {
                return;
            }

            RunningJobs.Add(nameof(UpdateUsersToContactForFeedback));
            try
            {
                List<User> users = await DataRepository.GetAll<User>()
                    .Where(
                        user => user.Status == UserStatuses.Active
                                && user.UserSettings.Any(us => us.Key == UserSettingKeys.AllowContact && us.Value.ToLower() == "true"))
                    .ToListAsync();
                var records = users.Select(
                        u => new {
                            u.Firstname,
                            u.Lastname,
                            u.JobTitle,
                            u.EmailAddress,
                            u.ContactFirstName,
                            u.ContactLastName,
                            u.ContactJobTitle,
                            u.ContactEmailAddress,
                            u.ContactPhoneNumber,
                            u.ContactOrganisation
                        })
                    .ToList();
                await Core.Classes.Extensions.SaveCSVAsync(FileRepository, records, filePath);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateUsersToContactForFeedback));
            }
        }

    }
}
