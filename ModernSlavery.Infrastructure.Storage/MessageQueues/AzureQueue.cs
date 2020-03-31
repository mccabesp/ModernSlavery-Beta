using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using ModernSlavery.Core.Interfaces;
using Newtonsoft.Json;

namespace ModernSlavery.Infrastructure.Storage.MessageQueues
{
    public class AzureQueue : IQueue
    {
        public virtual async Task AddMessageAsync<TInstance>(TInstance instance)
        {
            if (instance == null || Equals(instance, default(TInstance)))
                throw new ArgumentNullException(nameof(instance));

            var json = JsonConvert.SerializeObject(instance);

            await AddMessageAsync(json);
        }

        public virtual async Task AddMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(nameof(message));

            // Check if its a large message and that this queue supports it
            if (fileRepository == null && message.Length > CloudQueueMessage.MaxMessageSize)
                throw new ArgumentException(
                    $"{nameof(AzureQueue)}: Queue message exceeds maximum message size for azure queues. The message size was {message.Length}. Azure's maximum message size is {CloudQueueMessage.MaxMessageSize}.",
                    nameof(message));

            // Write the message to storage if it's too large
            if (fileRepository != null && message.Length > CloudQueueMessage.MaxMessageSize)
            {
                // Create a "message ID"
                var filePath = $"LargeQueueFiles\\{Name}\\{Guid.NewGuid()}.json";
                var bytes = Encoding.UTF8.GetBytes(message);
                await fileRepository.WriteAsync(filePath, bytes);
                message = $"file:{filePath}";
            }

            // Create the azure message
            var queueMessage = new CloudQueueMessage(message);

            // Get the queue via lazy loading
            var queue = await lazyQueue.Value;

            // Write the message to the azure queue
            await queue.AddMessageAsync(queueMessage);
        }

        private async Task<CloudQueue> ConnectToAzureQueueLazyAsync()
        {
            // Parse the connection string and return a reference to the storage account.
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create a CloudFileClient object for credentialed access to File storage.
            var queueClient = storageAccount.CreateCloudQueueClient();

            var queue = queueClient.GetQueueReference(Name);

            // Create the queue if it doesn't already exist
            await queue.CreateIfNotExistsAsync().ConfigureAwait(false);

            return queue;
        }

        #region Constructors

        /// <summary>
        ///     Enables large message support when providing a FileRepository.
        /// </summary>
        public AzureQueue(string connectionString, string queueName, IFileRepository fileRepository)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));

            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentNullException(nameof(queueName));

            this.connectionString = connectionString;
            Name = queueName;
            lazyQueue = new Lazy<Task<CloudQueue>>(async () => await ConnectToAzureQueueLazyAsync());

            this.fileRepository = fileRepository;
        }

        #endregion

        #region Dependencies

        private readonly Lazy<Task<CloudQueue>> lazyQueue;
        private readonly string connectionString;
        private readonly IFileRepository fileRepository;
        public string Name { get; }

        #endregion
    }
}