﻿using System;
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

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        /// <summary>
        ///     Handling healthy queued Stannp messages. After 5 failed attempts message is added to poisoned queue.
        /// </summary>
        /// <param name="queueMessage"></param>
        /// <param name="log"></param>
        [Singleton] //Ensures execution on only one instance
        [Disable(typeof(DisableWebjobProvider))]
        public async Task SendEmail([QueueTrigger(QueueNames.SendEmail)] string queueMessage, ILogger log)
        {
            var wrapper = JsonConvert.DeserializeObject<QueueWrapper>(queueMessage);
            wrapper.Message = Regex.Unescape(wrapper.Message).TrimI("\"");
            var messageType = typeof(SendGeoMessageModel).Assembly.GetType(wrapper.Type, true);
            var parameters = JsonConvert.DeserializeObject(wrapper.Message, messageType);

            if (parameters is SendGeoMessageModel)
            {
                var pars = (SendGeoMessageModel) parameters;
                if (!await _messenger.SendGeoMessageAsync(pars.subject, pars.message, pars.test).ConfigureAwait(false))
                    throw new Exception("Could not send email message to GEO for queued message:" + queueMessage);
            }
            else
            {
                try
                {
                    await _messenger.SendEmailTemplateAsync((EmailTemplate) parameters).ConfigureAwait(false);
                }
                catch
                {
                    throw new Exception($"Could not send email for unknown type '{wrapper.Type}'. Queued message:" +
                                        queueMessage);
                }
            }

            log.LogDebug($"Executed {nameof(SendEmail)}:{wrapper.Type} successfully");
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
            log.LogError($"Could not send email for queued message, Details:{queueMessage}");

            //Send Email to GEO reporting errors
            await _messenger.SendGeoMessageAsync("GPG - GOV WEBJOBS ERROR",
                "Could not send email for queued message:" + queueMessage).ConfigureAwait(false);
        }
    }
}