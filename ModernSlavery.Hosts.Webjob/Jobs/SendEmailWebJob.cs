using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.EmailTemplates;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using Newtonsoft.Json;
using ModernSlavery.Hosts.Webjob.Classes;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public class SendEmailWebJob : WebJob
    {
        #region Dependencies
        private readonly ISmtpMessenger _messenger;
        #endregion

        public SendEmailWebJob(ISmtpMessenger messenger)
        {
            _messenger = messenger;
        }

        /// <summary>
        ///     Handling healthy queued Stannp messages. After 5 failed attempts message is added to poisoned queue.
        /// </summary>
        /// <param name="queueMessage"></param>
        /// <param name="log"></param>
        [Singleton] //Ensures execution on only one instance
        [Disable(typeof(DisableWebJobProvider))]
        public async Task SendEmailAsync([QueueTrigger(QueueNames.SendEmail)] string queueMessage, ILogger log)
        {
            var wrapper = JsonConvert.DeserializeObject<QueueWrapper>(queueMessage);
            var record = wrapper.Record;
            var typeName = record == null ? "Unknown" : record.GetType().Name;

            if (record is SendMsuMessageModel sendMsuMessageModel)
            {
                if (!await _messenger.SendMsuMessageAsync(sendMsuMessageModel.subject, sendMsuMessageModel.message).ConfigureAwait(false))
                    throw new Exception("Could not send email message to GEO for queued message:" + queueMessage);
            }
            else if (record is EmailTemplate emailTemplate)
            {
                await _messenger.SendEmailTemplateAsync(emailTemplate).ConfigureAwait(false);
            }
            else
                throw new Exception($"Could not send email for unknown type '{typeName}'. Queued message:{queueMessage}");

            log.LogDebug($"Executed WebJob {nameof(SendEmailAsync)}:{typeName} successfully");
        }

        /// <summary>
        ///     Handling all email message sends
        /// </summary>
        /// <param name="queueMessage"></param>
        /// <param name="log"></param>
        [Singleton] //Ensures execution on only one instance
        public async Task SendEmailPoisonAsync([QueueTrigger(QueueNames.SendEmail + "-poison")]
            string queueMessage,
            ILogger log)
        {
            log.LogError(new Exception(), $"Could not send email for queued message, Details:{queueMessage}");

            //Send Email to GEO reporting errors
            await _messenger.SendMsuMessageAsync("MSU - GOV WEBJOBS ERROR",
                "Could not send email for queued message:" + queueMessage).ConfigureAwait(false);
        }
    }
}