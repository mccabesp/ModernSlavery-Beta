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
        public async Task UpdateScopes([TimerTrigger(typeof(EveryWorkingHourSchedule), RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                var filePath = Path.Combine(_SharedBusinessLogic.SharedOptions.DownloadsPath,
                    Filenames.OrganisationScopes);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateScopes)))
                {
                    var files = await _SharedBusinessLogic.FileRepository.GetFilesAsync(
                        _SharedBusinessLogic.SharedOptions.DownloadsPath,
                        $"{Path.GetFileNameWithoutExtension(Filenames.OrganisationScopes)}*{Path.GetExtension(Filenames.OrganisationScopes)}").ConfigureAwait(false);
                    if (files.Any()) return;
                }

                await UpdateScopesAsync(filePath).ConfigureAwait(false);

                log.LogDebug($"Executed {nameof(UpdateScopes)}:successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed {nameof(UpdateScopes)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                StartedJobs.Add(nameof(UpdateScopes));
            }
        }

        public async Task UpdateScopesAsync(string filePath)
        {
            if (RunningJobs.Contains(nameof(UpdateScopes))) return;

            RunningJobs.Add(nameof(UpdateScopes));
            try
            {
                await WriteRecordsPerYearAsync(
                        filePath,
                        year => Task.FromResult(_scopeBusinessLogic.GetScopesFileModelByYear(year).ToList()))
                    .ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateScopes));
            }
        }
    }
}