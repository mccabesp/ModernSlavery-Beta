using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.SharedKernel;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        public async Task UpdateUsersToSendInfo([TimerTrigger(typeof(EveryWorkingHourSchedule), RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                var filePath = Path.Combine(_SharedBusinessLogic.SharedOptions.DownloadsPath, Filenames.SendInfo);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateUsersToSendInfo)) &&
                    await _SharedBusinessLogic.FileRepository.GetFileExistsAsync(filePath)) return;

                await UpdateUsersToSendInfoAsync(filePath);
                log.LogDebug($"Executed {nameof(UpdateUsersToSendInfo)}:successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed {nameof(UpdateUsersToSendInfo)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
            finally
            {
                StartedJobs.Add(nameof(UpdateUsersToSendInfo));
            }
        }

        public async Task UpdateUsersToSendInfoAsync(string filePath)
        {
            if (RunningJobs.Contains(nameof(UpdateUsersToSendInfo))) return;

            RunningJobs.Add(nameof(UpdateUsersToSendInfo));
            try
            {
                var users = await _SharedBusinessLogic.DataRepository.GetAll<User>().Where(user =>
                        user.Status == UserStatuses.Active
                        && user.UserSettings.Any(us =>
                            us.Key == UserSettingKeys.SendUpdates && us.Value.ToLower() == "true"))
                    .ToListAsync();
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
                await Extensions.SaveCSVAsync(_SharedBusinessLogic.FileRepository, records, filePath);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateUsersToSendInfo));
            }
        }
    }
}