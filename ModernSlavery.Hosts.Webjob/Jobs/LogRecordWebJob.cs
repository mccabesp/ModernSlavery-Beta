using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models.LogModels;
using Newtonsoft.Json;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Hosts.Webjob.Classes;

namespace ModernSlavery.Hosts.Webjob.Jobs
{

    public class LogRecordWebJob : WebJob
    {
        #region Dependencies
        private readonly ISmtpMessenger _messenger;
        private readonly SharedOptions _sharedOptions;
        private readonly IFileRepository _fileRepository;
        #endregion

        public LogRecordWebJob(
            ISmtpMessenger messenger,
            SharedOptions sharedOptions,
            IFileRepository fileRepository)
        {
            _messenger = messenger;
            _sharedOptions = sharedOptions;
            _fileRepository = fileRepository;
        }

        private static readonly ConcurrentDictionary<string, SemaphoreSlim> FileLocks =
                new ConcurrentDictionary<string, SemaphoreSlim>(StringComparer.OrdinalIgnoreCase);

        [Singleton(Mode = SingletonMode.Listener)] //Ensures execution on only one instance with one listener
        [Disable(typeof(DisableWebJobProvider))]
        public async Task LogRecordAsync([QueueTrigger(QueueNames.LogRecord)] string queueMessage, ILogger log)
        {
            //Retrieve long messages from file storage
            var filepath = GetLargeQueueFilepath(queueMessage);
            if (!string.IsNullOrWhiteSpace(filepath))
                queueMessage = await _fileRepository.ReadAsync(filepath).ConfigureAwait(false);

            //Get the log event details
            var wrapper = JsonConvert.DeserializeObject<LogRecordWrapperModel>(queueMessage);

            //Calculate the daily log file path
            var LogRoot = _sharedOptions.LogPath;
            var FilePath = Path.Combine(LogRoot, wrapper.FileName);

            //Ensure the directory exists
            var DailyPath = Path.Combine(Path.GetPathRoot(FilePath), Path.GetDirectoryName(FilePath));

            if (!await _fileRepository.GetDirectoryExistsAsync(DailyPath).ConfigureAwait(false))
                await _fileRepository.CreateDirectoryAsync(DailyPath).ConfigureAwait(false);

            DailyPath = Path.Combine(
                Path.GetPathRoot(FilePath),
                Path.GetDirectoryName(FilePath),
                Path.GetFileNameWithoutExtension(FilePath) + "_" + VirtualDateTime.Now.ToString("yyMMdd") +
                Path.GetExtension(FilePath));

            if (!FileLocks.ContainsKey(DailyPath)) FileLocks[DailyPath] = new SemaphoreSlim(1, 1);

            var fileLock = FileLocks[DailyPath];

            //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
            await fileLock.WaitAsync().ConfigureAwait(false);
            try
            {
                FileLocks[DailyPath] = fileLock;

                //Write to the log entry
                await _fileRepository.AppendCsvRecordAsync(DailyPath, wrapper.Record).ConfigureAwait(false);
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                fileLock.Release();
            }

            //Delete the large file
            if (!string.IsNullOrWhiteSpace(filepath))
                await _fileRepository.DeleteFileAsync(filepath).ConfigureAwait(false);

            log.LogDebug($"Executed WebJob {nameof(LogRecordAsync)}:{queueMessage} successfully");
        }

        public async Task LogRecordPoison([QueueTrigger(QueueNames.LogRecord + "-poison")]
            string queueMessage,
            ILogger log)
        {
            //Retrieve long messages from file storage
            var filepath = GetLargeQueueFilepath(queueMessage);
            if (!string.IsNullOrWhiteSpace(filepath))
            {
                //Get the large file
                queueMessage = await _fileRepository.ReadAsync(filepath).ConfigureAwait(false);
                //Delete the large file
                await _fileRepository.DeleteFileAsync(filepath).ConfigureAwait(false);
            }

            log.LogError(new Exception(), $"Could not log record: Details:{queueMessage}");

            //Send Email to GEO reporting errors
            await _messenger.SendMsuMessageAsync("MSU - GOV WEBJOBS ERROR", "Could not log record:" + queueMessage).ConfigureAwait(false);
        }

        private static string GetLargeQueueFilepath(string queueMessage)
        {
            if (!queueMessage.StartsWith("file:")) return null;

            var filepath = queueMessage.AfterFirst("file:", includeWhenNoSeparator: false);
            return filepath;
        }
    }
}