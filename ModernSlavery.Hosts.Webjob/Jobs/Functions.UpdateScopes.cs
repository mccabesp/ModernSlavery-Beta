using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.SharedKernel;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {

        public async Task UpdateScopes([TimerTrigger(typeof(Functions.EveryWorkingHourSchedule), RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                string filePath = Path.Combine(_SharedBusinessLogic.SharedOptions.DownloadsPath, Filenames.OrganisationScopes);

                //Dont execute on startup if file already exists
                if (!Functions.StartedJobs.Contains(nameof(UpdateScopes)))
                {
                    IEnumerable<string> files = await _SharedBusinessLogic.FileRepository.GetFilesAsync(
                        _SharedBusinessLogic.SharedOptions.DownloadsPath,
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
                Functions.StartedJobs.Add(nameof(UpdateScopes));
            }
        }

        public async Task UpdateScopesAsync(string filePath)
        {
            if (Functions.RunningJobs.Contains(nameof(UpdateScopes)))
            {
                return;
            }

            Functions.RunningJobs.Add(nameof(UpdateScopes));
            try
            {
                await WriteRecordsPerYearAsync(
                        filePath,
                        year => Task.FromResult(Enumerable.ToList<ScopesFileModel>(_ScopeBusinessLogic.GetScopesFileModelByYear(year))))
                    .ConfigureAwait(false);
            }
            finally
            {
                Functions.RunningJobs.Remove(nameof(UpdateScopes));
            }
        }

    }
}
