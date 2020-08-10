using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        [Disable(typeof(DisableWebjobProvider))]
        public async Task UpdateOrganisationLateSubmissions(
            [TimerTrigger(typeof(EveryWorkingHourSchedule), RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                var filePath = Path.Combine(_SharedBusinessLogic.SharedOptions.DownloadsPath,
                    Filenames.OrganisationLateSubmissions);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateOrganisationLateSubmissions))
                    && await _SharedBusinessLogic.FileRepository.GetFileExistsAsync(filePath).ConfigureAwait(false))
                    return;

                await UpdateOrganisationLateSubmissionsAsync(filePath, log).ConfigureAwait(false);

                log.LogDebug($"Executed {nameof(UpdateOrganisationLateSubmissions)}:successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed {nameof(UpdateOrganisationLateSubmissions)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                StartedJobs.Add(nameof(UpdateOrganisationLateSubmissions));
            }
        }

        private async Task UpdateOrganisationLateSubmissionsAsync(string filePath, ILogger log)
        {
            var callingMethodName = nameof(UpdateOrganisationLateSubmissions);
            if (RunningJobs.Contains(callingMethodName)) return;

            RunningJobs.Add(callingMethodName);
            try
            {
                var records = _SubmissionBusinessLogic.GetLateSubmissions();
                await Extensions.SaveCSVAsync(_SharedBusinessLogic.FileRepository, records, filePath).ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(callingMethodName);
            }
        }
    }
}