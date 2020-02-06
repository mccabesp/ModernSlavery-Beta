using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.Core.Classes.Queues
{

    public class LogRecordQueue : AzureQueue
    {

        public LogRecordQueue(string connectionString, IFileRepository fileRepo)
            : base(connectionString, QueueNames.LogRecord, fileRepo) { }

    }

}
