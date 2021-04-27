using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.File;
using Microsoft.Azure.Storage.Queue;
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
            AccessKey = _storageAccount.Credentials.ExportBase64EncodedKey();
        }
        public string AccessKey { get; }


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

        public async Task<Uri> GetContainerDirectoryUriAsync(string containerName, string relativeAddress="/")
        {
            if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentNullException(nameof(containerName));

            // Create a CloudFileClient object for credentialed access to File storage.
            var client = _storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);
            if (!await container.ExistsAsync()) throw new Exception($"Container '{containerName}' does not exist");
            var directoryReference = container.GetDirectoryReference(relativeAddress);
            return directoryReference?.StorageUri == null ? null : directoryReference.StorageUri.PrimaryUri ?? directoryReference.StorageUri.SecondaryUri;
        }

        public async Task CreateContainerAsync(string containerName)
        {
            if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentNullException(nameof(containerName));

            // Create a CloudFileClient object for credentialed access to File storage.
            var client = _storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();
        }

        public async IAsyncEnumerable<Uri> ListBlobUrisAsync(string containerName)
        {
            if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentNullException(nameof(containerName));

            // Create a CloudFileClient object for credentialed access to File storage.
            var client = _storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);
            if (!await container.ExistsAsync()) yield break;
            var token = new BlobContinuationToken();
            var blobs = await container.ListBlobsSegmentedAsync(token).ConfigureAwait(false);
            foreach (var blob in blobs.Results)
                yield return blob.StorageUri.PrimaryUri ?? blob.StorageUri.SecondaryUri;
        }

        public async Task<bool> DeleteBlobAsync(string containerName, Uri storageUri)
        {
            if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentNullException(nameof(containerName));
            if (storageUri==null) throw new ArgumentNullException(nameof(storageUri));


            // Create a CloudFileClient object for credentialed access to File storage.
            var client = _storageAccount.CreateCloudBlobClient();
            var blob = await client.GetBlobReferenceFromServerAsync(storageUri);

            return await blob.DeleteIfExistsAsync();
        }

        public async Task<bool> DeleteBlobAsync(string containerName, string blobName)
        {
            if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentNullException(nameof(containerName));
            if (string.IsNullOrWhiteSpace(blobName)) throw new ArgumentNullException(nameof(blobName));

            // Create a CloudFileClient object for credentialed access to File storage.
            var client = _storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);
            if (!await container.ExistsAsync()) return false;

            var blob = container.GetBlobReference(blobName);
            return await blob.DeleteIfExistsAsync();
        }

        public async Task<Stream> OpenBlobStreamAsync(string containerName, Uri storageUri)
        {
            if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentNullException(nameof(containerName));
            if (storageUri == null) throw new ArgumentNullException(nameof(storageUri));

            // Create a CloudFileClient object for credentialed access to File storage.
            var client = _storageAccount.CreateCloudBlobClient();
            var blob = await client.GetBlobReferenceFromServerAsync(storageUri) as CloudBlob;

            if (!await blob.ExistsAsync()) throw new FileNotFoundException($"Cannot find blob '{blob.Name}' in container '{containerName}'");
            return await blob.OpenReadAsync();
        }
        public async Task UploadBlobStreamAsync(Stream stream, string containerName, Uri storageUri)
        {
            if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentNullException(nameof(containerName));
            if (storageUri == null) throw new ArgumentNullException(nameof(storageUri));

            // Create a CloudFileClient object for credentialed access to File storage.
            var client = _storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlockBlobReference(Path.GetFileName(storageUri.ToString()));
            await blob.UploadFromStreamAsync(stream);
        }
    }
}
