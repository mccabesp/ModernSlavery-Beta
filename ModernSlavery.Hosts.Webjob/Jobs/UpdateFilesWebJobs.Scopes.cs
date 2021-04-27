using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class UpdateFilesWebJobs
    {
        [Disable(typeof(DisableWebJobProvider))]
        public async Task UpdateScopesAsync([TimerTrigger(typeof(EveryWorkingHourSchedule))]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                var filePath = Path.Combine(_sharedOptions.DownloadsPath,
                    Filenames.OrganisationScopes);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateScopesAsync)))
                {
                    var files = await _fileRepository.GetFilesAsync(
                        _sharedOptions.DownloadsPath,
                        $"{Path.GetFileNameWithoutExtension(Filenames.OrganisationScopes)}*{Path.GetExtension(Filenames.OrganisationScopes)}").ConfigureAwait(false);
                    if (files.Any()) return;
                }

                await UpdateScopesAsync(filePath).ConfigureAwait(false);

                log.LogDebug($"Executed WebJob {nameof(UpdateScopesAsync)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed {nameof(UpdateScopesAsync)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("MSU - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                StartedJobs.Add(nameof(UpdateScopesAsync));
            }
        }

        public async Task UpdateScopesAsync(string filePath)
        {
            if (RunningJobs.Contains(nameof(UpdateScopesAsync))) return;

            RunningJobs.Add(nameof(UpdateScopesAsync));
            try
            {
                await WriteRecordsPerYearAsync(
                        filePath,
                        year => Task.FromResult(_scopeBusinessLogic.GetScopesFileModelByYear(year).ToList()))
                    .ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateScopesAsync));
            }
        }
    }
}