using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.SharedKernel;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {

        public async Task UpdateUsersToSendInfo([TimerTrigger(typeof(Functions.EveryWorkingHourSchedule), RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                string filePath = Path.Combine(_CommonBusinessLogic.GlobalOptions.DownloadsPath, Filenames.SendInfo);

                //Dont execute on startup if file already exists
                if (!Functions.StartedJobs.Contains(nameof(UpdateUsersToSendInfo)) && await _CommonBusinessLogic.FileRepository.GetFileExistsAsync(filePath))
                {
                    return;
                }

                await UpdateUsersToSendInfoAsync(filePath);
                log.LogDebug($"Executed {nameof(UpdateUsersToSendInfo)}:successfully");
            }
            catch (Exception ex)
            {
                string message = $"Failed {nameof(UpdateUsersToSendInfo)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
            finally
            {
                Functions.StartedJobs.Add(nameof(UpdateUsersToSendInfo));
            }
        }

        public async Task UpdateUsersToSendInfoAsync(string filePath)
        {
            if (Functions.RunningJobs.Contains(nameof(UpdateUsersToSendInfo)))
            {
                return;
            }

            Functions.RunningJobs.Add(nameof(UpdateUsersToSendInfo));
            try
            {
                List<User> users = await Queryable.Where<User>(_CommonBusinessLogic.DataRepository.GetAll<User>(), user => user.Status == UserStatuses.Active
                                                                                                                        && Enumerable.Any<UserSetting>(user.UserSettings, us => us.Key == UserSettingKeys.SendUpdates && us.Value.ToLower() == "true"))
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
                await Core.Classes.Extensions.SaveCSVAsync(_CommonBusinessLogic.FileRepository, records, filePath);
            }
            finally
            {
                Functions.RunningJobs.Remove(nameof(UpdateUsersToSendInfo));
            }
        }

    }
}
