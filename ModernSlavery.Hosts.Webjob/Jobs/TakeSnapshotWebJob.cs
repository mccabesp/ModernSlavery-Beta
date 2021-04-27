using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using static ModernSlavery.Core.Extensions.Web;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;
using ModernSlavery.Hosts.Webjob.Classes;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public class TakeSnapshotWebJob : WebJob
    {
        #region Dependencies
        private readonly StorageOptions _storageOptions;
        private readonly ISmtpMessenger _messenger;
        private readonly SharedOptions _sharedOptions;
        #endregion

        public TakeSnapshotWebJob(
            StorageOptions storageOptions,
            ISmtpMessenger messenger,
            SharedOptions sharedOptions)
        {
            _storageOptions = storageOptions;
            _messenger = messenger;
            _sharedOptions = sharedOptions;
        }

        [Disable(typeof(DisableWebJobProvider))]
        public async Task TakeSnapshotAsync([TimerTrigger(typeof(MidnightSchedule))]
            TimerInfo timer,
                ILogger log)
        {
            if (RunningJobs.Contains(nameof(TakeSnapshotAsync))) return;
            RunningJobs.Add(nameof(TakeSnapshotAsync));
            try
            {
                var deletedCount = 0;

                var azureStorageConnectionString = _storageOptions.AzureConnectionString;
                if (azureStorageConnectionString.Equals("UseDevelopmentStorage=true")) return;

                var connectionString = azureStorageConnectionString.ConnectionStringToDictionary();

                var azureStorageAccount = connectionString["AccountName"];
                var azureStorageKey = connectionString["AccountKey"];
                var azureStorageShareName = _storageOptions.AzureShareName;

                //Take the snapshot
                await TakeSnapshotAsync(azureStorageAccount, azureStorageKey, azureStorageShareName).ConfigureAwait(false);

                //Get the list of snapshots
                var response = await ListSnapshotsAsync(azureStorageAccount, azureStorageKey, azureStorageShareName).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var xml = XElement.Parse(response);
                    var snapshots =
                        xml.Descendants().Where(e => e.Name.LocalName.EqualsI("Snapshot")).Select(e => e.Value)
                            .ToList();
                    //var snapshots = snapshots.Where(e => e.EqualsI("Snapshot")).Select(e=>e.Value).ToList();
                    var deadline = VirtualDateTime.Now.AddDays(0 - _sharedOptions.MaxSnapshotDays);
                    foreach (var snapshot in snapshots)
                    {
                        var date = DateTime.Parse(snapshot);
                        if (date > deadline) continue;

                        await DeleteSnapshotAsync(log, azureStorageAccount, azureStorageKey, azureStorageShareName, snapshot).ConfigureAwait(false);
                        deletedCount++;
                    }
                }

                log.LogDebug($"Executed WebJob {nameof(TakeSnapshotAsync)} successfully. Deleted: {deletedCount} stale snapshots");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob:{nameof(TakeSnapshotAsync)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("MSU - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                RunningJobs.Remove(nameof(TakeSnapshotAsync));
            }

        }

        public async Task TakeSnapshotAsync(ILogger log)
        {
            try
            {
                var azureStorageConnectionString = _storageOptions.AzureConnectionString;
                if (azureStorageConnectionString.Equals("UseDevelopmentStorage=true")) return;

                var connectionString = azureStorageConnectionString.ConnectionStringToDictionary();

                var azureStorageAccount = connectionString["AccountName"];
                var azureStorageKey = connectionString["AccountKey"];
                var azureStorageShareName = _storageOptions.AzureShareName;

                //Take the snapshot
                await TakeSnapshotAsync(azureStorageAccount, azureStorageKey, azureStorageShareName).ConfigureAwait(false);

                log.LogDebug($"Executed WebJob {nameof(TakeSnapshotAsync)} successfully");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Failed webjob:{nameof(TakeSnapshotAsync)}:{ex.Message}");
                throw;
            }
        }

        private async Task<string> TakeSnapshotAsync(string storageAccount, string storageKey, string shareName)
        {
            var version = "2017-04-17";
            var comp = "snapshot";
            var restype = "share";
            var dt = VirtualDateTime.UtcNow;
            var StringToSign = "PUT\n"
                               + "\n" // content encoding
                               + "\n" // content language
                               + "\n" // content length
                               + "\n" // content md5
                               + "\n" // content type
                               + "\n" // date
                               + "\n" // if modified since
                               + "\n" // if match
                               + "\n" // if none match
                               + "\n" // if unmodified since
                               + "\n" // range
                               + "x-ms-date:"
                               + dt.ToString("R")
                               + "\nx-ms-version:"
                               + version
                               + "\n" // headers
                               + $"/{storageAccount}/{shareName}\ncomp:{comp}\nrestype:{restype}";

            var signature = SignAuthHeader(StringToSign, storageKey, storageAccount);
            var authorizationHeader = $"SharedKey {storageAccount}:{signature}";
            var url = $"https://{storageAccount}.file.core.windows.net/{shareName}?restype={restype}&comp={comp}";

            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-date", dt.ToString("R"));
            headers.Add("x-ms-version", version);
            headers.Add("Authorization", authorizationHeader);
            var json = await WebRequestAsync(HttpMethods.Put, url, headers: headers).ConfigureAwait(false);
            return headers["x-ms-snapshot"];
        }

        private async Task<string> ListSnapshotsAsync(string storageAccount, string storageKey, string shareName)
        {
            var version = "2017-04-17";
            var comp = "list";
            var dt = VirtualDateTime.UtcNow;
            var StringToSign = "GET\n"
                               + "\n" // content encoding
                               + "\n" // content language
                               + "\n" // content length
                               + "\n" // content md5
                               + "\n" // content type
                               + "\n" // date
                               + "\n" // if modified since
                               + "\n" // if match
                               + "\n" // if none match
                               + "\n" // if unmodified since
                               + "\n" // range
                               + "x-ms-date:"
                               + dt.ToString("R")
                               + "\nx-ms-version:"
                               + version
                               + "\n" // headers
                               + $"/{storageAccount}/\ncomp:{comp}\ninclude:snapshots\nprefix:{shareName}";

            var signature = SignAuthHeader(StringToSign, storageKey, storageAccount);
            var authorizationHeader = $"SharedKey {storageAccount}:{signature}";
            var url =
                $"https://{storageAccount}.file.core.windows.net/?comp={comp}&prefix={shareName}&include=snapshots";

            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-date", dt.ToString("R"));
            headers.Add("x-ms-version", version);
            headers.Add("Authorization", authorizationHeader);
            var response = await WebRequestAsync(HttpMethods.Get, url, headers: headers).ConfigureAwait(false);
            return response;
        }

        private async Task<string> DeleteSnapshotAsync(ILogger log,
            string storageAccount,
            string storageKey,
            string shareName,
            string snapshot)
        {
            var version = "2017-04-17";
            var restype = "share";
            var dt = VirtualDateTime.UtcNow;
            var StringToSign = "DELETE\n"
                               + "\n" // content encoding
                               + "\n" // content language
                               + "\n" // content length
                               + "\n" // content md5
                               + "\n" // content type
                               + "\n" // date
                               + "\n" // if modified since
                               + "\n" // if match
                               + "\n" // if none match
                               + "\n" // if unmodified since
                               + "\n" // range
                               + "x-ms-date:"
                               + dt.ToString("R")
                               + "\nx-ms-version:"
                               + version
                               + "\n" // headers
                               + $"/{storageAccount}/{shareName}\nrestype:{restype}\nsharesnapshot:{snapshot}";

            var signature = SignAuthHeader(StringToSign, storageKey, storageAccount);
            var authorizationHeader = $"SharedKey {storageAccount}:{signature}";
            var url =
                $"https://{storageAccount}.file.core.windows.net/{shareName}?sharesnapshot={snapshot}&restype={restype}";

            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-date", dt.ToString("R"));
            headers.Add("x-ms-version", version);
            headers.Add("Authorization", authorizationHeader);
            var response = await WebRequestAsync(HttpMethods.Delete, url, headers: headers).ConfigureAwait(false);

            log.LogDebug($"{nameof(DeleteSnapshotAsync)}: successfully deleted snapshot:{snapshot}");

            return headers["x-ms-request-id"];
        }

        private string SignAuthHeader(string canonicalizedString, string key, string account)
        {
            var unicodeKey = Convert.FromBase64String(key);
            using (var hmacSha256 = new HMACSHA256(unicodeKey))
            {
                var dataToHmac = Encoding.UTF8.GetBytes(canonicalizedString);
                return Convert.ToBase64String(hmacSha256.ComputeHash(dataToHmac));
            }
        }

    }
}