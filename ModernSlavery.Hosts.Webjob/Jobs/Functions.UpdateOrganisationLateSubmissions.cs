using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.SharedKernel;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
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
                    && await _SharedBusinessLogic.FileRepository.GetFileExistsAsync(filePath))
                    return;

                await UpdateOrganisationLateSubmissionsAsync(filePath, log);

                log.LogDebug($"Executed {nameof(UpdateOrganisationLateSubmissions)}:successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed {nameof(UpdateOrganisationLateSubmissions)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
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
                await Extensions.SaveCSVAsync(_SharedBusinessLogic.FileRepository, records, filePath);
            }
            finally
            {
                RunningJobs.Remove(callingMethodName);
            }
        }
    }
}