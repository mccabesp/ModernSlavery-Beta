using System;
using Microsoft.Extensions.Logging;

namespace ModernSlavery.Core.Models.LogModels
{
    [Serializable]
    public class LogEventWrapperModel
    {
        public string ApplicationName { get; set; }
        public LogLevel LogLevel { get; set; }
        public LogEntryModel LogEntry { get; set; }
    }
}