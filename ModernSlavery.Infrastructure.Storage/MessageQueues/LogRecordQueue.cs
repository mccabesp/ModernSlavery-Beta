using ModernSlavery.Core;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.Infrastructure.Storage.MessageQueues
{
    public class LogRecordQueue : AzureQueue
    {
        public LogRecordQueue(string connectionString, IFileRepository fileRepo)
            : base(connectionString, QueueNames.LogRecord, fileRepo)
        {
        }
    }
}