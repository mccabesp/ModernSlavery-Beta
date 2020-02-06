using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.Core.Classes.Queues
{

    public class LogEventQueue : AzureQueue
    {

        public LogEventQueue(string connectionString, IFileRepository fileRepo) : base(connectionString, QueueNames.LogEvent, fileRepo) { }

    }

}
