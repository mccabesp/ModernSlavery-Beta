using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ModernSlavery.Core.Interfaces
{
    public interface IAzureStorageManager
    {
        string AccessKey { get; }

        Task ClearQueueAsync(string queueName, DateTime? startDate = null, DateTime? endDate = null);
        Task CreateContainerAsync(string containerName);
        Task<bool> DeleteBlobAsync(string containerName, Uri storageUri);
        Task<bool> DeleteBlobAsync(string containerName, string blobName);
        Task<Uri> GetContainerDirectoryUriAsync(string containerName, string relativeAddress = "/");
        IAsyncEnumerable<Uri> ListBlobUrisAsync(string containerName);
        IAsyncEnumerable<string> ListFileSharesAsync();
        IAsyncEnumerable<(string name, int? messageCount)> ListQueuesAsync();
        Task<Stream> OpenBlobStreamAsync(string containerName, Uri storageUri);
        Task UploadBlobStreamAsync(Stream stream, string containerName, Uri storageUri);
    }
}