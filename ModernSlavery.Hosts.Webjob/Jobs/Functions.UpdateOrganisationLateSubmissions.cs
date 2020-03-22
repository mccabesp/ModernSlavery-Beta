using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessLogic.Models;
using ModernSlavery.Core.SharedKernel;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {

        public async Task UpdateOrganisationLateSubmissions([TimerTrigger(typeof(Functions.EveryWorkingHourSchedule), RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                string filePath = Path.Combine(_SharedBusinessLogic.SharedOptions.DownloadsPath, Filenames.OrganisationLateSubmissions);

                //Dont execute on startup if file already exists
                if (!Functions.StartedJobs.Contains(nameof(UpdateOrganisationLateSubmissions))
                    && await _SharedBusinessLogic.FileRepository.GetFileExistsAsync(filePath))
                {
                    return;
                }

                await UpdateOrganisationLateSubmissionsAsync(filePath, log);

                log.LogDebug($"Executed {nameof(UpdateOrganisationLateSubmissions)}:successfully");
            }
            catch (Exception ex)
            {
                string message = $"Failed {nameof(UpdateOrganisationLateSubmissions)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
            finally
            {
                Functions.StartedJobs.Add(nameof(UpdateOrganisationLateSubmissions));
            }
        }

        private async Task UpdateOrganisationLateSubmissionsAsync(string filePath, ILogger log)
        {
            string callingMethodName = nameof(UpdateOrganisationLateSubmissions);
            if (Functions.RunningJobs.Contains(callingMethodName))
            {
                return;
            }

            Functions.RunningJobs.Add(callingMethodName);
            try
            {
                IEnumerable<LateSubmissionsFileModel> records = _SubmissionBusinessLogic.GetLateSubmissions();
                await Core.Classes.Extensions.SaveCSVAsync(_SharedBusinessLogic.FileRepository, records, filePath);
            }
            finally
            {
                Functions.RunningJobs.Remove(callingMethodName);
            }
        }

    }
}
