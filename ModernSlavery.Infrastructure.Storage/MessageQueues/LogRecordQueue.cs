using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel;

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