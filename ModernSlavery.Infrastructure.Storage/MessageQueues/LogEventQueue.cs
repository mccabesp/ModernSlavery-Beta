using ModernSlavery.Core;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.Infrastructure.Storage.MessageQueues
{
    public class LogEventQueue : AzureQueue
    {
        public LogEventQueue(string connectionString, IFileRepository fileRepo) : base(connectionString,QueueNames.LogEvent, fileRepo)
        {
        }
    }
}