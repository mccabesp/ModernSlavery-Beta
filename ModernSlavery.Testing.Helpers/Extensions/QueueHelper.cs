using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using ModernSlavery.Core.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class QueueHelper
    {
        public static async Task ClearQueuesAsync(this IServiceProvider serviceProvider)
        {
            var storageOptions = serviceProvider.GetRequiredService<StorageOptions>();

            var queues = ListQueuesAsync(storageOptions.AzureConnectionString);
            await foreach (var queue in queues)
                await queue.ClearAsync().ConfigureAwait(false);
        }


        public static async IAsyncEnumerable<CloudQueue> ListQueuesAsync(string connectionString)
        {
            // Parse the connection string and return a reference to the storage account.
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create a CloudFileClient object for credentialed access to File storage.
            var queueClient = storageAccount.CreateCloudQueueClient();

            // Initialize a new token
            var continuationToken = new QueueContinuationToken();

            do
            {
                var segment = await queueClient.ListQueuesSegmentedAsync(continuationToken).ConfigureAwait(false);
                continuationToken = segment.ContinuationToken;

                foreach (var queue in segment.Results)
                    yield return queue;
            }
            while (continuationToken != null);
        }
    }
}
