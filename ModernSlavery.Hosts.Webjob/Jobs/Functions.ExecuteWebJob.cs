using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using Newtonsoft.Json;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        [Singleton(Mode = SingletonMode.Listener)] //Ensures execution on only one instance with one listener
        [Disable(typeof(DisableWebjobProvider))]
        public async Task ExecuteWebjob([QueueTrigger(QueueNames.ExecuteWebJob)]
            string queueMessage,
            ILogger log)
        {
            var wrapper = JsonConvert.DeserializeObject<QueueWrapper>(queueMessage);
            wrapper.Message = JsonConvert.DeserializeObject<string>(wrapper.Message);
            wrapper.Message = Regex.Unescape(wrapper.Message).TrimI("\"");

            var parameters = wrapper.Message.FromQueryString();
            var command = parameters["command"];
            parameters.Remove("command");
            if (string.IsNullOrWhiteSpace(command)) command = wrapper.Message;

            switch (command)
            {
                case "UpdateFile":
                    await UpdateFileAsync(log, parameters["filePath"], parameters["action"]).ConfigureAwait(false);
                    break;
                default:
                    var inputs = new Dictionary<string, object> { { "timer", null }, { "log", log } };
                    foreach (string key in parameters.Keys)
                        inputs[key] = parameters[key];
                    await _jobHost.CallAsync(command, inputs).ConfigureAwait(false);
                    break;
            }

            log.LogDebug($"Executed Webjob {nameof(ExecuteWebjob)}:{command} successfully");
        }

        public async Task ExecuteWebjobPoisonAsync([QueueTrigger(QueueNames.ExecuteWebJob + "-poison")]
            string queueMessage,
            ILogger log)
        {
            log.LogError($"Could not execute Webjob, Details: {queueMessage}");

            //Send Email to GEO reporting errors
            await _messenger.SendMsuMessageAsync("GPG - GOV WEBJOBS ERROR", "Could not execute Webjob:" + queueMessage).ConfigureAwait(false);
        }
    }
}