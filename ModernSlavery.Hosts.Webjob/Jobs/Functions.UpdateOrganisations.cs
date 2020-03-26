using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.SharedKernel;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        public async Task UpdateOrganisationsAsync([TimerTrigger(typeof(EveryWorkingHourSchedule), RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                var filePath = Path.Combine(_SharedBusinessLogic.SharedOptions.DownloadsPath, Filenames.Organisations);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateOrganisationsAsync)))
                {
                    var files = await _SharedBusinessLogic.FileRepository.GetFilesAsync(
                        _SharedBusinessLogic.SharedOptions.DownloadsPath,
                        $"{Path.GetFileNameWithoutExtension(Filenames.Organisations)}*{Path.GetExtension(Filenames.Organisations)}");
                    if (files.Any()) return;
                }

                await UpdateOrganisationsAsync(filePath);

                log.LogDebug($"Executed {nameof(UpdateOrganisationsAsync)}:successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed {nameof(UpdateOrganisationsAsync)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
            finally
            {
                StartedJobs.Add(nameof(UpdateOrganisationsAsync));
            }
        }

        public async Task UpdateOrganisationsAsync(string filePath)
        {
            if (RunningJobs.Contains(nameof(UpdateOrganisationsAsync))) return;

            RunningJobs.Add(nameof(UpdateOrganisationsAsync));
            try
            {
                await WriteRecordsPerYearAsync(
                    filePath,
                    _OrganisationBusinessLogic.GetOrganisationsFileModelByYearAsync);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateOrganisationsAsync));
            }
        }
    }
}