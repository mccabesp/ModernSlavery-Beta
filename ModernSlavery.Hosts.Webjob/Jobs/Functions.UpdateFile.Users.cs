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
using Extensions = ModernSlavery.Core.Classes.Extensions;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        [Disable(typeof(DisableWebjobProvider))]
        public async Task UpdateUsers([TimerTrigger(typeof(EveryWorkingHourSchedule))]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                var filePath = Path.Combine(_sharedOptions.DownloadsPath, Filenames.Users);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateUsers)) &&
                    await _fileRepository.GetFileExistsAsync(filePath).ConfigureAwait(false)) return;

                await UpdateUsersAsync(filePath).ConfigureAwait(false);
                log.LogDebug($"Executed {nameof(UpdateUsers)}:successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed {nameof(UpdateUsers)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                StartedJobs.Add(nameof(UpdateUsers));
            }
        }

        public async Task UpdateUsersAsync(string filePath)
        {
            if (RunningJobs.Contains(nameof(UpdateUsers))) return;

            RunningJobs.Add(nameof(UpdateUsers));
            try
            {
                var users = await _dataRepository.GetAll<User>().ToListAsync().ConfigureAwait(false);
                var records = users.Where(u => !_authorisationBusinessLogic.IsAdministrator(u))
                    .OrderBy(u => u.Lastname)
                    .Select(
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
                await Extensions.SaveCSVAsync(_fileRepository, records, filePath).ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateUsers));
            }
        }
    }
}