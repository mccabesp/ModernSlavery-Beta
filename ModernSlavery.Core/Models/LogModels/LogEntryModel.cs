using System;
using ModernSlavery.Extensions;

namespace ModernSlavery.Core.Models.LogModels
{
    [Serializable]
    public class LogEntryModel
    {
        public DateTime Date { get; set; } = VirtualDateTime.Now;
        public string Machine { get; set; } = $"Machine:{Environment.MachineName}";
        public string HttpMethod { get; set; }
        public string WebPath { get; set; }
        public string RemoteIP { get; set; }
        public string Source { get; set; } = AppDomain.CurrentDomain.FriendlyName;
        public string Message { get; set; }
        public string Details { get; set; }
        public string Stacktrace { get; set; }
    }
}