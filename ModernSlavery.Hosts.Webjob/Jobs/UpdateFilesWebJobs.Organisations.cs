﻿using System;
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
        public async Task UpdateOrganisationsAsync([TimerTrigger(typeof(EveryWorkingHourSchedule))]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                var filePath = Path.Combine(_sharedOptions.DownloadsPath, Filenames.Organisations);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateOrganisationsAsync)))
                {
                    var files = await _fileRepository.GetFilesAsync(
                        _sharedOptions.DownloadsPath,
                        $"{Path.GetFileNameWithoutExtension(Filenames.Organisations)}*{Path.GetExtension(Filenames.Organisations)}").ConfigureAwait(false);
                    if (files.Any()) return;
                }

                await UpdateOrganisationsAsync(filePath).ConfigureAwait(false);

                log.LogDebug($"Executed WebJob {nameof(UpdateOrganisationsAsync)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed {nameof(UpdateOrganisationsAsync)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("MSU - WEBJOBS ERROR", message).ConfigureAwait(false);
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
                    _organisationBusinessLogic.GetOrganisationFileModelByYearAsync).ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateOrganisationsAsync));
            }
        }
    }
}