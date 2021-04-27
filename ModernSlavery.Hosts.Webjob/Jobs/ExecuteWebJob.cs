using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using Newtonsoft.Json;
using ModernSlavery.Hosts.Webjob.Classes;
using System;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public class ExecuteWebJob : WebJob
    {
        #region Dependencies
        private readonly IJobHost _jobHost;
        private readonly ISmtpMessenger _messenger;
        #endregion

        public ExecuteWebJob(
            IJobHost jobHost,
            ISmtpMessenger messenger)
        {
            _jobHost = jobHost;
            _messenger = messenger;
        }

        [Singleton(Mode = SingletonMode.Listener)] //Ensures execution on only one instance with one listener
        [Disable(typeof(DisableWebJobProvider))]
        public async Task ExecuteAsync([QueueTrigger(QueueNames.ExecuteWebJob)]string queueMessage, ILogger log)
        {
            var wrapper = JsonConvert.DeserializeObject<QueueWrapper>(queueMessage);

            var record = wrapper.Record;
            var typeName = record==null ? "Unknown" : record.GetType().Name;

            string command;
            if (record is string pars)
            {
                var parameters = pars.FromQueryString();
                command = parameters["command"];
                if (string.IsNullOrWhiteSpace(command))
                {
                    command = pars;
                    parameters.Clear();
                }
                else
                {
                    parameters.Remove("command");
                }

                var arguments = new Dictionary<string, object> { { "timer", null }, { "log", log } };
                foreach (string key in parameters.Keys)
                    arguments[key] = parameters[key];

                await _jobHost.CallAsync(command, arguments).ConfigureAwait(false);
            }
            else
                throw new Exception($"Could not execute webjob for unknown type '{typeName}'. Queued message:{queueMessage}");


            log.LogDebug($"Executed WebJob {nameof(ExecuteWebJob)}:{command} successfully");
        }

        public async Task ExecutePoisonAsync([QueueTrigger(QueueNames.ExecuteWebJob + "-poison")]
                string queueMessage,
            ILogger log)
        {
            log.LogError(new Exception(), $"Could not execute WebJob, Details: {queueMessage}");

            //Send Email to GEO reporting errors
            await _messenger.SendMsuMessageAsync("MSU - GOV WEBJOBS ERROR", "Could not execute WebJob:" + queueMessage).ConfigureAwait(false);
        }
    }
}