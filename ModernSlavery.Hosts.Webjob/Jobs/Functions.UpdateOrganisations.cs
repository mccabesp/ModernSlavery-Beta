using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {

        public async Task UpdateOrganisationsAsync([TimerTrigger(typeof(Functions.EveryWorkingHourSchedule), RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                string filePath = Path.Combine(_SharedBusinessLogic.SharedOptions.DownloadsPath, Filenames.Organisations);

                //Dont execute on startup if file already exists
                if (!Functions.StartedJobs.Contains(nameof(UpdateOrganisationsAsync)))
                {
                    IEnumerable<string> files = await _SharedBusinessLogic.FileRepository.GetFilesAsync(
                        _SharedBusinessLogic.SharedOptions.DownloadsPath,
                        $"{Path.GetFileNameWithoutExtension(Filenames.Organisations)}*{Path.GetExtension(Filenames.Organisations)}");
                    if (files.Any())
                    {
                        return;
                    }
                }

                await UpdateOrganisationsAsync(filePath);

                log.LogDebug($"Executed {nameof(UpdateOrganisationsAsync)}:successfully");
            }
            catch (Exception ex)
            {
                string message = $"Failed {nameof(UpdateOrganisationsAsync)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
            finally
            {
                Functions.StartedJobs.Add(nameof(UpdateOrganisationsAsync));
            }
        }

        public async Task UpdateOrganisationsAsync(string filePath)
        {
            if (Functions.RunningJobs.Contains(nameof(UpdateOrganisationsAsync)))
            {
                return;
            }

            Functions.RunningJobs.Add(nameof(UpdateOrganisationsAsync));
            try
            {
                await WriteRecordsPerYearAsync<OrganisationsFileModel>(
                    filePath,
                    _OrganisationBusinessLogic.GetOrganisationsFileModelByYearAsync);
            }
            finally
            {
                Functions.RunningJobs.Remove(nameof(UpdateOrganisationsAsync));
            }
        }

    }
}
