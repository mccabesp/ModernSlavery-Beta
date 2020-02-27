﻿using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ModernSlavery.SharedKernel;

namespace ModernSlavery.WebJob
{
    public partial class Functions
    {

        [Singleton(Mode = SingletonMode.Listener)] //Ensures execution on only one instance with one listener
        public async Task ExecuteWebjob([QueueTrigger(QueueNames.ExecuteWebJob)]
            string queueMessage,
            ILogger log)
        {
            var wrapper = JsonConvert.DeserializeObject<QueueWrapper>(queueMessage);
            wrapper.Message = JsonConvert.DeserializeObject<string>(wrapper.Message);
            wrapper.Message = Regex.Unescape(wrapper.Message).TrimI("\"");

            NameValueCollection parameters = wrapper.Message.FromQueryString();
            string command = parameters["command"];
            if (string.IsNullOrWhiteSpace(command))
            {
                command = wrapper.Message;
            }

            switch (command)
            {
                case "CompaniesHouseCheck":
                    await CompaniesHouseCheckAsync(log, true);
                    break;
                case "DnBImport":
                    await DnBImportAsync(log, parameters["currentUserId"].ToInt64(-1));
                    break;
                case "UpdateFile":
                    await UpdateFileAsync(log, parameters["filePath"], parameters["action"]);
                    break;
                case "UpdateSearch":
                    await UpdateSearchAsync(log, parameters["userEmail"], true);
                    break;
                case "UpdateDownloadFiles":
                    await UpdateDownloadFilesAsync(log, parameters["userEmail"], true);
                    break;
                case "TakeSnapshot":
                    await TakeSnapshotAsync(log);
                    break;
                case "FixOrganisationsNames":
                    await FixOrganisationsNamesAsync(log, parameters["userEmail"], parameters["comment"]);
                    break;
                default:
                    throw new Exception("Could not execute webjob:" + queueMessage);
            }

            log.LogDebug($"Executed {nameof(ExecuteWebjob)}:{command} successfully");
        }

        public async Task ExecuteWebjobPoisonAsync([QueueTrigger(QueueNames.ExecuteWebJob + "-poison")]
            string queueMessage,
            ILogger log)
        {
            log.LogError($"Could not execute Webjob, Details: {queueMessage}");

            //Send Email to GEO reporting errors
            await _Messenger.SendGeoMessageAsync("GPG - GOV WEBJOBS ERROR", "Could not execute Webjob:" + queueMessage);
        }

    }
}
