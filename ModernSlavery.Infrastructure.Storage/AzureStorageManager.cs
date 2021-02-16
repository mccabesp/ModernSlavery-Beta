using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage.Queue;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Storage
{
    public class AzureStorageManager : IAzureStorageManager
    {
        private readonly CloudStorageAccount _storageAccount;
        private CloudQueueClient _queueClient;

        public AzureStorageManager(StorageOptions storageOptions)
        {
            if (storageOptions==null) throw new ArgumentNullException(nameof(storageOptions));
            if (string.IsNullOrWhiteSpace(storageOptions.AzureConnectionString)) throw new ArgumentNullException(nameof(storageOptions.AzureConnectionString));

            _storageAccount = CloudStorageAccount.Parse(storageOptions.AzureConnectionString);
        }

        public async IAsyncEnumerable<(string name, int? messageCount)> ListQueuesAsync()
        {
            // Create a CloudFileClient object for credentialed access to File storage.
            _queueClient = _queueClient ?? (_queueClient = _storageAccount.CreateCloudQueueClient());

            var token = new QueueContinuationToken();
            var queues = await _queueClient.ListQueuesSegmentedAsync(token).ConfigureAwait(false);
            foreach (var queue in queues.Results)
                yield return (queue.Name, queue.ApproximateMessageCount);
        }

        public virtual async Task ClearQueueAsync(string queueName, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (startDate >= DateTime.Now) throw new ArgumentOutOfRangeException(nameof(startDate), $"{nameof(startDate)} cannot be later than now");
            if (endDate >= DateTime.Now) throw new ArgumentOutOfRangeException(nameof(endDate), $"{nameof(endDate)} cannot be later than now");
            if (startDate != null && endDate != null && startDate >= endDate) throw new ArgumentOutOfRangeException(nameof(startDate), $"{nameof(startDate)} must be less than {nameof(endDate)}");

            // Create a CloudFileClient object for credentialed access to File storage.
            _queueClient = _queueClient ?? (_queueClient = _storageAccount.CreateCloudQueueClient());

            // Get the queue 
            var queue = _queueClient.GetQueueReference(queueName);

            // Clear the queue
            if (startDate == null && endDate == null)
                await queue.ClearAsync().ConfigureAwait(false);
            else
            {
                var messages = await queue.GetMessagesAsync(queue.ApproximateMessageCount.Value).ConfigureAwait(false);

                Parallel.ForEach(messages, async message =>
                {
                    if (startDate != null || endDate != null)
                    {
                        var modified = message.InsertionTime;

                        if (startDate != null && modified < startDate) return;
                        if (endDate != null && modified > endDate) return;
                    }

                    await queue.DeleteMessageAsync(message).ConfigureAwait(false);
                });
            }
        }

        public async IAsyncEnumerable<string> ListFileSharesAsync()
        {
            // Create a CloudFileClient object for credentialed access to File storage.
            var client = _storageAccount.CreateCloudFileClient();
            var token = new FileContinuationToken();
            var shares = await client.ListSharesSegmentedAsync(token).ConfigureAwait(false);
            foreach (var share in shares.Results)
                yield return share.Name;
        }
    }
}
