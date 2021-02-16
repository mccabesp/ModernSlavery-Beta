using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModernSlavery.Core.Interfaces
{
    public interface IAzureStorageManager
    {
        Task ClearQueueAsync(string queueName, DateTime? startDate = null, DateTime? endDate = null);
        IAsyncEnumerable<string> ListFileSharesAsync();
        IAsyncEnumerable<(string name, int? messageCount)> ListQueuesAsync();
    }
}