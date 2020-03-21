using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.SharedKernel;

namespace ModernSlavery.WebJob
{
    public partial class Functions
    {

        public async Task UpdateScopes([TimerTrigger(typeof(EveryWorkingHourSchedule), RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                string filePath = Path.Combine(_CommonBusinessLogic.GlobalOptions.DownloadsPath, Filenames.OrganisationScopes);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateScopes)))
                {
                    IEnumerable<string> files = await _CommonBusinessLogic.FileRepository.GetFilesAsync(
                        _CommonBusinessLogic.GlobalOptions.DownloadsPath,
                        $"{Path.GetFileNameWithoutExtension(Filenames.OrganisationScopes)}*{Path.GetExtension(Filenames.OrganisationScopes)}");
                    if (files.Any())
                    {
                        return;
                    }
                }

                await UpdateScopesAsync(filePath);

                log.LogDebug($"Executed {nameof(UpdateScopes)}:successfully");
            }
            catch (Exception ex)
            {
                string message = $"Failed {nameof(UpdateScopes)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
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
            if (RunningJobs.Contains(nameof(UpdateScopes)))
            {
                return;
            }

            RunningJobs.Add(nameof(UpdateScopes));
            try
            {
                await WriteRecordsPerYearAsync(
                        filePath,
                        year => Task.FromResult(_ScopeBusinessLogic.GetScopesFileModelByYear(year).ToList()))
                    .ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateScopes));
            }
        }

    }
}
