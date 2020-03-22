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

        public async Task UpdateUsers([TimerTrigger(typeof(Functions.EveryWorkingHourSchedule), RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                string filePath = Path.Combine(_CommonBusinessLogic.GlobalOptions.DownloadsPath, Filenames.Users);

                //Dont execute on startup if file already exists
                if (!Functions.StartedJobs.Contains(nameof(UpdateUsers)) && await _CommonBusinessLogic.FileRepository.GetFileExistsAsync(filePath))
                {
                    return;
                }

                await UpdateUsersAsync(filePath);
                log.LogDebug($"Executed {nameof(UpdateUsers)}:successfully");
            }
            catch (Exception ex)
            {
                string message = $"Failed {nameof(UpdateUsers)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
            finally
            {
                Functions.StartedJobs.Add(nameof(UpdateUsers));
            }
        }

        public async Task UpdateUsersAsync(string filePath)
        {
            if (Functions.RunningJobs.Contains(nameof(UpdateUsers)))
            {
                return;
            }

            Functions.RunningJobs.Add(nameof(UpdateUsers));
            try
            {
                List<User> users = await EntityFrameworkQueryableExtensions.ToListAsync<User>(_CommonBusinessLogic.DataRepository.GetAll<User>());
                var records = users.Where(u => !u.IsAdministrator())
                    .OrderBy(u => u.Lastname)
                    .Select(
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
                            u.ContactOrganisation,
                            u.EmailVerifySendDate,
                            u.EmailVerifiedDate,
                            VerifyUrl = u.GetVerifyUrl(),
                            PasswordResetUrl = u.GetPasswordResetUrl(),
                            u.Status,
                            u.StatusDate,
                            u.StatusDetails,
                            u.Created
                        })
                    .ToList();
                await Core.Classes.Extensions.SaveCSVAsync(_CommonBusinessLogic.FileRepository, records, filePath);
            }
            finally
            {
                Functions.RunningJobs.Remove(nameof(UpdateUsers));
            }
        }

    }
}
