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
        public async Task UpdateUsersAsync([TimerTrigger(typeof(EveryWorkingHourSchedule))]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                var filePath = Path.Combine(_sharedOptions.DownloadsPath, Filenames.Users);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateUsersAsync)) &&
                    await _fileRepository.GetFileExistsAsync(filePath).ConfigureAwait(false)) return;

                await UpdateUsersAsync(filePath).ConfigureAwait(false);
                log.LogDebug($"Executed WebJob {nameof(UpdateUsersAsync)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed {nameof(UpdateUsersAsync)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("MSU - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                StartedJobs.Add(nameof(UpdateUsersAsync));
            }
        }

        public async Task UpdateUsersAsync(string filePath)
        {
            if (RunningJobs.Contains(nameof(UpdateUsersAsync))) return;

            RunningJobs.Add(nameof(UpdateUsersAsync));
            try
            {
                var users = await _dataRepository.GetAll<User>().Where(u=>u.UserId > 0).ToListAsync().ConfigureAwait(false);
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

                if (records.Any())await Extensions.SaveCSVAsync(_fileRepository, records, filePath).ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateUsersAsync));
            }
        }
    }
}