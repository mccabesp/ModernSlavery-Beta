using ModernSlavery.Core.Interfaces;
using ModernSlavery.SharedKernel;

namespace ModernSlavery.Infrastructure.Queue
{

    public class LogRecordQueue : AzureQueue
    {

        public LogRecordQueue(string connectionString, IFileRepository fileRepo)
            : base(connectionString, QueueNames.LogRecord, fileRepo) { }

    }

}
