using ModernSlavery.Core.Interfaces;
using ModernSlavery.SharedKernel;

namespace ModernSlavery.Infrastructure.Queue
{

    public class LogEventQueue : AzureQueue
    {

        public LogEventQueue(string connectionString, IFileRepository fileRepo) : base(connectionString, QueueNames.LogEvent, fileRepo) { }

    }

}
