using ModernSlavery.Core.Interfaces;
using ModernSlavery.SharedKernel;

namespace ModernSlavery.Infrastructure.Storage.MessageQueues
{
    public class LogEventQueue : AzureQueue
    {
        public LogEventQueue(string connectionString, IFileRepository fileRepo) : base(connectionString,
            QueueNames.LogEvent, fileRepo)
        {
        }
    }
}