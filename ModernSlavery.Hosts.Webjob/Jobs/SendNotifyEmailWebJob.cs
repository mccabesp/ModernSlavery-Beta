using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using ModernSlavery.Core;
using ModernSlavery.Core.Models;
using Newtonsoft.Json;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public class SendNotifyEmailWebJob : WebJob
    {
        #region Dependencies
        private readonly IGovNotifyAPI _govNotifyApi;
        #endregion

        public SendNotifyEmailWebJob(IGovNotifyAPI govNotifyApi)
        {
            _govNotifyApi = govNotifyApi;
        }

        /// <summary>
        ///     Handling healthy queued Notify email messages. After 5 failed attempts message is added to poisoned queue.
        /// </summary>
        /// <param name="queueMessage"></param>
        /// <param name="log"></param>
        [Singleton] //Ensures execution on only one instance
        [Disable(typeof(DisableWebJobProvider))]
        public async Task SendNotifyEmailAsync([QueueTrigger(QueueNames.SendNotifyEmail)]CloudQueueMessage queueMessage,ILogger log)
        {
            SendEmailRequest notifyEmail;
            try
            {
                notifyEmail = JsonConvert.DeserializeObject<SendEmailRequest>(queueMessage.AsString);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "EMAIL FAILURE: Failed to deserialise Notify email from queue");
                throw;
            }

            await _govNotifyApi.SendEmailAsync(notifyEmail).ConfigureAwait(false);
            log.LogDebug($"Executed WebJob {nameof(SendNotifyEmailAsync)} successfully");
        }

        /// <summary>
        ///     Handling poison-queued Notify email message.
        /// </summary>
        /// <param name="queueMessage"></param>
        /// <param name="log"></param>
        [Singleton] //Ensures execution on only one instance
        public async Task SendNotifyEmailPoisonAsync([QueueTrigger(QueueNames.SendNotifyEmail + "-poison")]
            string queueMessage,
            ILogger log)
        {
            log.LogError(new Exception(), $"EMAIL FAILURE: Notify email in poison queue. Details: {queueMessage}");
        }
    }
}