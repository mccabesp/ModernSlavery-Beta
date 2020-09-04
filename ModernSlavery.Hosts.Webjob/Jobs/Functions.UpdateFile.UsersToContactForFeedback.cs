using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        [Disable(typeof(DisableWebjobProvider))]
        public async Task UpdateUsersToContactForFeedback(
            [TimerTrigger(typeof(EveryWorkingHourSchedule))]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                var filePath = Path.Combine(_sharedOptions.DownloadsPath, Filenames.AllowFeedback);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateUsersToContactForFeedback))
                    && await _fileRepository.GetFileExistsAsync(filePath).ConfigureAwait(false))
                    return;

                await UpdateUsersToContactForFeedbackAsync(filePath).ConfigureAwait(false);
                log.LogDebug($"Executed {nameof(UpdateUsersToContactForFeedback)}:successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed {nameof(UpdateUsersToContactForFeedback)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
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
            if (RunningJobs.Contains(nameof(UpdateUsersToContactForFeedback))) return;

            RunningJobs.Add(nameof(UpdateUsersToContactForFeedback));
            try
            {
                var users = await _dataRepository.GetAll<User>().Where(user =>
                        user.Status == UserStatuses.Active
                        && user.UserSettings.Any(us =>
                            us.Key == UserSettingKeys.AllowContact && us.Value.ToLower() == "true"))
                    .ToListAsync().ConfigureAwait(false);
                var records = users.Select(
                        u => new
                        {
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
                await Extensions.SaveCSVAsync(_fileRepository, records, filePath).ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateUsersToContactForFeedback));
            }
        }
    }
}