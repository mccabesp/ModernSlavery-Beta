using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        [Disable(typeof(DisableWebjobProvider))]
        public async Task UpdateSubmissions([TimerTrigger(typeof(EveryWorkingHourSchedule))]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                var filePath = Path.Combine(_sharedOptions.DownloadsPath,
                    Filenames.OrganisationSubmissions);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateSubmissions))
                    && await _fileRepository.GetAnyFileExistsAsync(
                        _sharedOptions.DownloadsPath,
                        $"{Path.GetFileNameWithoutExtension(Filenames.OrganisationSubmissions)}*{Path.GetExtension(Filenames.OrganisationSubmissions)}").ConfigureAwait(false)
                )
                    return;

                await UpdateSubmissionsAsync(filePath).ConfigureAwait(false);

                log.LogDebug($"Executed Webjob {nameof(UpdateSubmissions)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed {nameof(UpdateSubmissions)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                StartedJobs.Add(nameof(UpdateSubmissions));
            }
        }

        public async Task UpdateSubmissionsAsync(string filePath)
        {
            if (RunningJobs.Contains(nameof(UpdateSubmissions))) return;

            RunningJobs.Add(nameof(UpdateSubmissions));
            try
            {
                await WriteRecordsPerYearAsync(
                        filePath,
                        year => Task.FromResult(_submissionBusinessLogic.GetStatementsFileModelByYear(year).ToList()))
                    .ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateSubmissions));
            }
        }
    }
}