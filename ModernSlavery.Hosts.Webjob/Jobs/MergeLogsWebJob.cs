using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Hosts.Webjob.Classes;
using System.IO.Compression;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public class MergeLogsWebJob : WebJob
    {
        #region Dependencies
        private readonly ISmtpMessenger _messenger;
        private readonly SharedOptions _sharedOptions;
        private readonly IFileRepository _fileRepository;
        #endregion

        public MergeLogsWebJob(
            ISmtpMessenger messenger,
            SharedOptions sharedOptions,
            IFileRepository fileRepository)
        {
            _messenger = messenger;
            _sharedOptions = sharedOptions;
            _fileRepository = fileRepository;
        }

        //Merge all event log files from all instances into 1 single file per month
        [Disable(typeof(DisableWebJobProvider))]
        public async Task MergeLogsAsync([TimerTrigger("%MergeLogs%")]TimerInfo timer,ILogger log)
        {
            if (RunningJobs.Contains(nameof(MergeLogsAsync))) return;
            RunningJobs.Add(nameof(MergeLogsAsync));
            try
            {
                //Backup the log files first
                await ArchiveAzureStorageAsync().ConfigureAwait(false);

                await MergeCsvLogsAsync<BadSicLogModel>(log, _sharedOptions.LogPath, Core.Filenames.BadSicLog).ConfigureAwait(false);
                await MergeCsvLogsAsync<EmailSendLogModel>(log, _sharedOptions.LogPath, Core.Filenames.EmailSendLog).ConfigureAwait(false);
                await MergeCsvLogsAsync<ManualChangeLogModel>(log, _sharedOptions.LogPath, Core.Filenames.ManualChangeLog).ConfigureAwait(false);
                await MergeCsvLogsAsync<RegisterLogModel>(log, _sharedOptions.LogPath, Core.Filenames.RegistrationLog).ConfigureAwait(false);
                await MergeCsvLogsAsync<SearchLogModel>(log, _sharedOptions.LogPath, Core.Filenames.SearchLog).ConfigureAwait(false);
                await MergeCsvLogsAsync<SubmissionLogModel>(log, _sharedOptions.LogPath, Core.Filenames.SubmissionLog).ConfigureAwait(false);
                await MergeCsvLogsAsync<UserLogModel>(log, _sharedOptions.LogPath, Core.Filenames.UserLog).ConfigureAwait(false);

                log.LogDebug($"Executed WebJob {nameof(MergeLogsAsync)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(MergeLogsAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("MSU - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                RunningJobs.Remove(nameof(MergeLogsAsync));
            }


        }

        private async Task MergeCsvLogsAsync<T>(ILogger log, string logPath, string filename)
        {
            //Ensure the directory exists
            if (!await _fileRepository.GetDirectoryExistsAsync(logPath).ConfigureAwait(false))
                await _fileRepository.CreateDirectoryAsync(logPath).ConfigureAwait(false);

            string prefix = Path.GetFileNameWithoutExtension(filename);
            string extension = Path.GetExtension(filename);

            //Get all the daily log files
            var files = await _fileRepository.GetFilesAsync(logPath, $"{prefix}_*{extension}").ConfigureAwait(false);
            var fileList = files.OrderBy(o => o).ToList();

            //Get all files before today
            var startDate = VirtualDateTime.Now.Date;

            foreach (var file in fileList)
                try
                {
                    //Get the date from this daily log filename
                    var dateSuffix = Path.GetFileNameWithoutExtension(file).AfterFirst("_");
                    if (string.IsNullOrWhiteSpace(dateSuffix) || dateSuffix.Length < 6) continue;

                    var date = dateSuffix.FromShortestDateTime();

                    //Ignore log files with no date in the filename
                    if (date == DateTime.MinValue) continue;

                    //Ignore todays daily log file 
                    if (date.Date >= startDate) continue;

                    //Get the monthly log file for this files date
                    var monthLog = Path.Combine(logPath, $"{prefix}_{date:yyMM}{extension}");

                    //Read all the records from this daily log file
                    var records = await _fileRepository.ReadCSVAsync<T>(file).ConfigureAwait(false);

                    //Add the records to its monthly log file
                    await _fileRepository.AppendCsvRecordsAsync(monthLog, records).ConfigureAwait(false);

                    //Delete this daily log file
                    await _fileRepository.DeleteFileAsync(file).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    var message =
                        $"ERROR: WebJob ({nameof(MergeLogsAsync)}) could not merge file '{file}':{ex.Message}:{ex.GetDetailsText()}";
                    log.LogError(ex, message);
                }

            var archiveDeadline = VirtualDateTime.Now.AddYears(-1).Date;

            foreach (var file in fileList)
                try
                {
                    //Get the date from this daily log filename
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var dateSuffix = fileName.AfterFirst("_");
                    if (string.IsNullOrWhiteSpace(dateSuffix)) continue;

                    if (dateSuffix.Length != 4) continue;

                    //Get the date of this log from the filename
                    var year = dateSuffix.Substring(0, 2).ToInt32().ToFourDigitYear();
                    var month = dateSuffix.Substring(2, 2).ToInt32();
                    var logDate = new DateTime(year, month, 1);

                    //Dont archive logs newer than 1 year
                    if (logDate >= archiveDeadline) continue;

                    var archivePath = Path.Combine(logPath, year.ToString());
                    if (!await _fileRepository.GetDirectoryExistsAsync(archivePath).ConfigureAwait(false))
                        await _fileRepository.CreateDirectoryAsync(archivePath).ConfigureAwait(false);

                    //Ensure we have a unique filename
                    var ext = Path.GetExtension(file);
                    var archiveFilePath = Path.Combine(archivePath, fileName) + ext;

                    var c = 0;
                    while (await _fileRepository.GetFileExistsAsync(archiveFilePath).ConfigureAwait(false))
                    {
                        c++;
                        archiveFilePath = Path.Combine(archivePath, fileName) + $" ({c}){ext}";
                    }

                    //Copy to the archive folder
                    await _fileRepository.CopyFileAsync(file, archiveFilePath, false).ConfigureAwait(false);

                    //Delete the old file
                    if (await _fileRepository.GetFileExistsAsync(archiveFilePath).ConfigureAwait(false))
                        await _fileRepository.DeleteFileAsync(file).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    var message =
                        $"ERROR: WebJob ({nameof(MergeLogsAsync)}) could not archive file '{file}':{ex.Message}:{ex.GetDetailsText()}";
                    log.LogError(ex, message);
                }
        }

        public async Task ArchiveAzureStorageAsync()
        {

            string logZipDir = @$"{_sharedOptions.ArchivePath.TrimI(" /\\")}\";

            //Ensure the archive directory exists
            if (!await _fileRepository.GetDirectoryExistsAsync(logZipDir).ConfigureAwait(false))
                await _fileRepository.CreateDirectoryAsync(logZipDir).ConfigureAwait(false);

            //Create the zip file path using todays date
            var logZipFilePath = Path.Combine(logZipDir, $"{VirtualDateTime.Now.ToString("yyyyMMdd")}.zip");

            //Dont zip if we have one for today
            if (await _fileRepository.GetFileExistsAsync(logZipFilePath).ConfigureAwait(false)) return;

            var tempfile = new FileInfo(Path.GetTempFileName());
            tempfile.Delete();
            var files = 0;
            try
            {
                using (var zipFile = ZipFile.Open(tempfile.FullName, ZipArchiveMode.Create))
                {
                    foreach (var file in await _fileRepository.GetFilesAsync("\\", "*.*", true).ConfigureAwait(false))
                    {
                        var dirFile = Url.UrlToDirSeparator(file).TrimI("\\/");

                        if (dirFile.StartsWithI(logZipDir)) continue;

                        // prevents stdout_ logs
                        if (dirFile.ContainsI("stdout_")) continue;

                        var entry = zipFile.CreateEntry(dirFile);
                        using (var entryStream = entry.Open())
                        {
                            await _fileRepository.ReadAsync(dirFile, entryStream).ConfigureAwait(false);
                            files++;
                        }
                    }
                }

                //Save CSV to storage
                if (files > 0)
                {
                    using var fileStream = File.OpenRead(tempfile.FullName);
                    await _fileRepository.WriteAsync(logZipFilePath, fileStream).ConfigureAwait(false);
                }
            }
            finally
            {
                if (tempfile.Exists) tempfile.Delete();
            }
        }

    }
}