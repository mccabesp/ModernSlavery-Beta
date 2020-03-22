using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessLogic.Models.FileModels;
using ModernSlavery.Core.SharedKernel;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {

        public async Task UpdateSubmissions([TimerTrigger(typeof(Functions.EveryWorkingHourSchedule), RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                string filePath = Path.Combine(_CommonBusinessLogic.GlobalOptions.DownloadsPath, Filenames.OrganisationSubmissions);

                //Dont execute on startup if file already exists
                if (!Functions.StartedJobs.Contains(nameof(UpdateSubmissions))
                    && await _CommonBusinessLogic.FileRepository.GetAnyFileExistsAsync(
                        _CommonBusinessLogic.GlobalOptions.DownloadsPath,
                        $"{Path.GetFileNameWithoutExtension(Filenames.OrganisationSubmissions)}*{Path.GetExtension(Filenames.OrganisationSubmissions)}")
                )
                {
                    return;
                }

                await UpdateSubmissionsAsync(filePath);

                log.LogDebug($"Executed {nameof(UpdateSubmissions)}:successfully");
            }
            catch (Exception ex)
            {
                string message = $"Failed {nameof(UpdateSubmissions)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
            finally
            {
                Functions.StartedJobs.Add(nameof(UpdateSubmissions));
            }
        }

        public async Task UpdateSubmissionsAsync(string filePath)
        {
            if (Functions.RunningJobs.Contains(nameof(UpdateSubmissions)))
            {
                return;
            }

            Functions.RunningJobs.Add(nameof(UpdateSubmissions));
            try
            {
                await WriteRecordsPerYearAsync(
                        filePath,
                        year => Task.FromResult(Enumerable.ToList<SubmissionsFileModel>(_SubmissionBusinessLogic.GetSubmissionsFileModelByYear(year))))
                    .ConfigureAwait(false);
            }
            finally
            {
                Functions.RunningJobs.Remove(nameof(UpdateSubmissions));
            }
        }

    }
}
