using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Hosts.Webjob.Classes;
using ModernSlavery.Core.Models;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public class ClearConsoleLogsWebJob : WebJob
    {
        #region Dependencies
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        public readonly int KeepConsoleLogDays;
        #endregion

        public ClearConsoleLogsWebJob(ILogger<ClearConsoleLogsWebJob> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            KeepConsoleLogDays = _configuration["Webjobs:KeepConsoleLogDays"].ToInt32();
        }

        [NoAutomaticTrigger]
        public void ClearConsoleLogs()
        {
            if (KeepConsoleLogDays < 0) return;
            try
            {
                var logsPath = _configuration["Filepaths:LogFiles"];
                logsPath = FileSystem.ExpandLocalPath(logsPath);
                var files = Directory.GetFiles(logsPath, "*.log").ToHashSet(StringComparer.OrdinalIgnoreCase);
                files.AddRange(Directory.GetFiles(logsPath, "*.xml"));
                var deletedFilenames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var logFile in files)
                {
                    try
                    {
                        if (File.Exists(logFile) && (KeepConsoleLogDays== 0 || File.GetLastWriteTimeUtc(logFile).AddDays(KeepConsoleLogDays) < VirtualDateTime.Now))
                        {
                            File.Delete(logFile);
                            deletedFilenames.Add(Path.GetFileName(logFile));
                        }
                    }
                    catch (IOException)
                    { 
                        //Skip locked files
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Could not delete log file '{logFile}'");
                    }
                }

                _logger.LogDebug($"Executed WebJob {nameof(ClearConsoleLogs)} successfully. Deleted {deletedFilenames.Count} log files: {deletedFilenames.ToDelimitedString(", ")}");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(ClearConsoleLogs)}):{ex.Message}:{ex.GetDetailsText()}";
                _logger.LogError(ex, message);
            }
        }
    }
}