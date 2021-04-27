using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using Extensions = ModernSlavery.Core.Classes.Extensions;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class UpdateFilesWebJobs
    {
        [Disable(typeof(DisableWebJobProvider))]
        public async Task UpdateUsersToSendInfoAsync([TimerTrigger(typeof(EveryWorkingHourSchedule))]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                var filePath = Path.Combine(_sharedOptions.DownloadsPath, Filenames.SendInfo);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateUsersToSendInfoAsync)) &&
                    await _fileRepository.GetFileExistsAsync(filePath).ConfigureAwait(false)) return;

                await UpdateUsersToSendInfoAsync(filePath).ConfigureAwait(false);
                log.LogDebug($"Executed WebJob {nameof(UpdateUsersToSendInfoAsync)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed {nameof(UpdateUsersToSendInfoAsync)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("MSU - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                StartedJobs.Add(nameof(UpdateUsersToSendInfoAsync));
            }
        }

        public async Task UpdateUsersToSendInfoAsync(string filePath)
        {
            if (RunningJobs.Contains(nameof(UpdateUsersToSendInfoAsync))) return;

            RunningJobs.Add(nameof(UpdateUsersToSendInfoAsync));
            try
            {
                var users = await _dataRepository.GetAll<User>().Where(user => user.UserId > 0 &&
                        user.Status == UserStatuses.Active
                        && user.UserSettings.Any(us =>
                            us.Key == UserSettingKeys.SendUpdates && us.Value.ToLower() == "true"))
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

                if (records.Any())await Extensions.SaveCSVAsync(_fileRepository, records, filePath).ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateUsersToSendInfoAsync));
            }
        }
    }
}