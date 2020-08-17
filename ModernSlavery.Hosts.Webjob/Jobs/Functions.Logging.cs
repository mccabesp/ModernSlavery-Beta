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

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> FileLocks =
            new ConcurrentDictionary<string, SemaphoreSlim>(StringComparer.OrdinalIgnoreCase);

        [Singleton(Mode = SingletonMode.Listener)] //Ensures execution on only one instance with one listener
        [Disable(typeof(DisableWebjobProvider))]
        public async Task LogEvent([QueueTrigger(QueueNames.LogEvent)] string queueMessage, ILogger log)
        {
            //Retrieve long messages from file storage
            var filepath = GetLargeQueueFilepath(queueMessage);
            if (!string.IsNullOrWhiteSpace(filepath))
                queueMessage = await _SharedBusinessLogic.FileRepository.ReadAsync(filepath).ConfigureAwait(false);

            var wrapper = JsonConvert.DeserializeObject<LogEventWrapperModel>(queueMessage);

            //Calculate the daily log file path
            var LogRoot = _SharedBusinessLogic.SharedOptions.LogPath;
            string FilePath;
            switch (wrapper.LogLevel)
            {
                case LogLevel.Trace:
                    FilePath = Path.Combine(LogRoot, wrapper.ApplicationName, "TraceLog.csv");
                    break;
                case LogLevel.Debug:
                    FilePath = Path.Combine(LogRoot, wrapper.ApplicationName, "DebugLog.csv");
                    break;
                case LogLevel.Information:
                    FilePath = Path.Combine(LogRoot, wrapper.ApplicationName, "InfoLog.csv");
                    break;
                case LogLevel.Warning:
                    FilePath = Path.Combine(LogRoot, wrapper.ApplicationName, "WarningLog.csv");
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    FilePath = Path.Combine(LogRoot, wrapper.ApplicationName, "ErrorLog.csv");
                    break;
                default:
                    throw new ArgumentException("Invalid Log Level", nameof(LogLevel));
            }

            var DailyPath = Path.Combine(
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
                //Write to the log entry
                await _SharedBusinessLogic.FileRepository.AppendCsvRecordAsync(DailyPath, wrapper.LogEntry).ConfigureAwait(false);
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                fileLock.Release();
            }


            //Delete the large file
            if (!string.IsNullOrWhiteSpace(filepath))
                await _SharedBusinessLogic.FileRepository.DeleteFileAsync(filepath).ConfigureAwait(false);

            log.LogDebug($"Executed {nameof(LogEvent)}:{queueMessage} successfully");
        }

        public async Task LogEventPoison([QueueTrigger(QueueNames.LogEvent + "-poison")]
            string queueMessage,
            ILogger log)
        {
            //Retrieve long messages from file storage
            var filepath = GetLargeQueueFilepath(queueMessage);
            if (!string.IsNullOrWhiteSpace(filepath))
            {
                //Get the large file
                queueMessage = await _SharedBusinessLogic.FileRepository.ReadAsync(filepath).ConfigureAwait(false);
                //Delete the large file
                await _SharedBusinessLogic.FileRepository.DeleteFileAsync(filepath).ConfigureAwait(false);
            }

            log.LogError($"Could not log event, Details: {queueMessage}");

            //Send Email to GEO reporting errors
            await _Messenger.SendGeoMessageAsync("GPG - GOV WEBJOBS ERROR", "Could not log event:" + queueMessage).ConfigureAwait(false);
        }

        [Singleton(Mode = SingletonMode.Listener)] //Ensures execution on only one instance with one listener
        [Disable(typeof(DisableWebjobProvider))]
        public async Task LogRecord([QueueTrigger(QueueNames.LogRecord)] string queueMessage, ILogger log)
        {
            //Retrieve long messages from file storage
            var filepath = GetLargeQueueFilepath(queueMessage);
            if (!string.IsNullOrWhiteSpace(filepath))
                queueMessage = await _SharedBusinessLogic.FileRepository.ReadAsync(filepath).ConfigureAwait(false);

            //Get the log event details
            var wrapper = JsonConvert.DeserializeObject<LogRecordWrapperModel>(queueMessage);

            //Calculate the daily log file path
            var LogRoot = _SharedBusinessLogic.SharedOptions.LogPath;
            var FilePath = Path.Combine(LogRoot, wrapper.ApplicationName, wrapper.FileName);
            var DailyPath = Path.Combine(
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
                await _SharedBusinessLogic.FileRepository.AppendCsvRecordAsync(DailyPath, wrapper.Record).ConfigureAwait(false);
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                fileLock.Release();
            }

            //Delete the large file
            if (!string.IsNullOrWhiteSpace(filepath))
                await _SharedBusinessLogic.FileRepository.DeleteFileAsync(filepath).ConfigureAwait(false);

            log.LogDebug($"Executed {nameof(LogRecord)}:{queueMessage} successfully");
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
                queueMessage = await _SharedBusinessLogic.FileRepository.ReadAsync(filepath).ConfigureAwait(false);
                //Delete the large file
                await _SharedBusinessLogic.FileRepository.DeleteFileAsync(filepath).ConfigureAwait(false);
            }

            log.LogError($"Could not log record: Details:{queueMessage}");

            //Send Email to GEO reporting errors
            await _Messenger.SendGeoMessageAsync("GPG - GOV WEBJOBS ERROR", "Could not log record:" + queueMessage).ConfigureAwait(false);
        }

        private static string GetLargeQueueFilepath(string queueMessage)
        {
            if (!queueMessage.StartsWith("file:")) return null;

            var filepath = queueMessage.AfterFirst("file:", includeWhenNoSeparator: false);
            return filepath;
        }
    }
}