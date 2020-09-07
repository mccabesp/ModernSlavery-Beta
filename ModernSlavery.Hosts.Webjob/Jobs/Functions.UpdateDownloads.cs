using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using Extensions = ModernSlavery.Core.Classes.Extensions;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        //Update data for viewing service
        [Disable(typeof(DisableWebjobProvider))]
        public async Task UpdateDownloadFiles([TimerTrigger("%UpdateDownloadFiles%")]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                await UpdateDownloadFilesAsync(log).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                //Send Email to GEO reporting errors
                await _messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", ex.Message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
        }

        //Update GPG download file
        public async Task UpdateDownloadFilesAsync(ILogger log, string userEmail = null, bool force = false)
        {
            if (RunningJobs.Contains(nameof(UpdateDownloadFiles))) return;

            try
            {
                var returnYears = _dataRepository.GetAll<Statement>()
                    .Where(r => r.Status == StatementStatuses.Submitted)
                    .Select(r => r.SubmissionDeadline.Year)
                    .Distinct()
                    .ToList();

                //Get the downloads location
                var downloadsLocation = _sharedOptions.DownloadsLocation;

                //Ensure we have a directory
                if (!await _fileRepository.GetDirectoryExistsAsync(downloadsLocation).ConfigureAwait(false))
                    await _fileRepository.CreateDirectoryAsync(downloadsLocation).ConfigureAwait(false);

                foreach (var year in returnYears)
                {
                    //If another server is already in process of creating a file then skip

                    var downloadFilePattern = $"GPGData_{year}-{year + 1}.csv";
                    var files = await _fileRepository.GetFilesAsync(downloadsLocation,
                        downloadFilePattern).ConfigureAwait(false);
                    var oldDownloadFilePath = files.FirstOrDefault();

                    //Skip if the file already exists and is newer than 1 hour or older than 1 year
                    if (oldDownloadFilePath != null && !force)
                    {
                        var lastWriteTime =
                            await _fileRepository.GetLastWriteTimeAsync(oldDownloadFilePath).ConfigureAwait(false);
                        if (lastWriteTime.AddHours(1) >= VirtualDateTime.Now ||
                            lastWriteTime.AddYears(2) <= VirtualDateTime.Now) continue;
                    }

                    var statements = await _dataRepository.GetAll<Statement>().Where(r =>
                            r.SubmissionDeadline.Year == year
                            && r.Status == StatementStatuses.Submitted
                            && r.Organisation.Status == OrganisationStatuses.Active)
                        .ToListAsync().ConfigureAwait(false);
                    statements.RemoveAll(r =>
                        r.Organisation.OrganisationName.StartsWithI(_sharedOptions.TestPrefix));

                    var downloadData = statements.ToList()
                        .Select(r => DownloadModel.Create(r))
                        .OrderBy(d => d.OrganisationName)
                        .ToList();

                    var newFilePath =
                        _fileRepository.GetFullPath(Path.Combine(downloadsLocation,
                            $"GPGData_{year}-{year + 1}.csv"));
                    try
                    {
                        if (downloadData.Any())
                            await Extensions.SaveCSVAsync(_fileRepository, downloadData,
                                newFilePath, oldDownloadFilePath).ConfigureAwait(false);
                        else if (!string.IsNullOrWhiteSpace(oldDownloadFilePath))
                            await _fileRepository.DeleteFileAsync(oldDownloadFilePath).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, ex.Message);
                    }
                }

                if (force && !string.IsNullOrWhiteSpace(userEmail))
                    try
                    {
                        await _messenger.SendMessageAsync(
                            "UpdateDownloadFiles complete",
                            userEmail,
                            "The update of the download files completed successfully.").ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, ex.Message);
                    }
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateDownloadFiles));
            }
        }
    }
}