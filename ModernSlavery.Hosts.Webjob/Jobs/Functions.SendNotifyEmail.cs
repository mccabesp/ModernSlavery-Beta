using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel;
using Newtonsoft.Json;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        /// <summary>
        ///     Handling healthy queued Notify email messages. After 5 failed attempts message is added to poisoned queue.
        /// </summary>
        /// <param name="queueMessage"></param>
        /// <param name="log"></param>
        [Singleton] //Ensures execution on only one instance
        public async Task SendNotifyEmail([QueueTrigger(QueueNames.SendNotifyEmail)]
            CloudQueueMessage queueMessage,
            ILogger log)
        {
            SendEmailRequest notifyEmail;
            try
            {
                notifyEmail = JsonConvert.DeserializeObject<SendEmailRequest>(queueMessage.AsString);
            }
            catch (Exception ex)
            {
                _CustomLogger.Error("EMAIL FAILURE: Failed to deserialise Notify email from queue", ex);
                throw;
            }

            govNotifyApi.SendEmail(notifyEmail);
            _CustomLogger.Information("Successfully received message from queue and passed to GovNotifyAPI", new {notifyEmail});
        }

        /// <summary>
        ///     Handling poison-queued Notify email message.
        /// </summary>
        /// <param name="queueMessage"></param>
        /// <param name="log"></param>
        [Singleton] //Ensures execution on only one instance
        public async Task SendNotifyEmailPoisonAsync([QueueTrigger(QueueNames.SendNotifyEmail + "-poison")]
            string queueMessage)
        {
            _CustomLogger.Error("EMAIL FAILURE: Notify email in poison queue", new {queueMessage});
        }

    }
}
