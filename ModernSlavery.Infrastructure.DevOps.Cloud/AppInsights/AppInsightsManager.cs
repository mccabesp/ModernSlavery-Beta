using System;
using System.Collections.Generic;
using ModernSlavery.Core.Extensions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Dynamic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ModernSlavery.Infrastructure.Azure.AppInsights
{
    public class AppInsightsManager
    {
        public readonly AzureManager AzureManager;

        public AppInsightsManager(AzureManager azureManager)
        {
            AzureManager = azureManager;
        }

        private const string apiVersion = "2015-05-01";

        public async IAsyncEnumerable<(string resourceName, string resourceGroup, string instrumentationKey)> ListAsync(string subscriptionId)
        {
            // See https://docs.microsoft.com/en-us/rest/api/application-insights/components/list

            //Create the uri
            var uri = $"https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.Insights/components?api-version={apiVersion}";
            //Create the authentication header
            var authenticationHeader = await AzureManager.GetAuthenticationHeaderAsync().ConfigureAwait(false);

            while (!string.IsNullOrWhiteSpace(uri))
            {
                //Execute the purge request
                var jsonResponse = await Web.WebRequestAsync(Web.HttpMethods.Get, uri, authenticationHeader).ConfigureAwait(false);

                //Get the status from the response
                if (string.IsNullOrWhiteSpace(jsonResponse)) throw new InvalidOperationException("No response returned");
                
                var response = JsonConvert.DeserializeObject<JObject>(jsonResponse);

                var value = response.GetValue("value");

                foreach (var item in value)
                {
                    string id = (string)item["id"];
                    string resourceGroup = id.AfterFirst("/resourceGroups/").BeforeFirst("/");
                    string resourceName = (string)item["name"];
                    string instrumentationKey = (string)item.SelectToken("properties.InstrumentationKey");
                    yield return (resourceName, resourceGroup, instrumentationKey);
                }
            }
        }

        public async Task<string> PurgeDataAsync(string subscriptionId, string resourceGroupName, string resourceName, string tableName)
        {
            //see https://docs.microsoft.com/en-us/rest/api/application-insights/components/purge

            //Create the uri
            var uri = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Insights/components/{resourceName}/purge?api-version={apiVersion}";

            //Create the body
            var body = new
            {
                table = tableName,
                filters = new List<dynamic>()
            };

            var now=DateTime.UtcNow.AddMinutes(5).ToString("s");
            //body.filters.Add(new { column = "timestamp", @operator = ">", value = "2019-01-01T00:00:00" });
            body.filters.Add(new { column = "timestamp", @operator = "<", value = now });

            var json = Json.SerializeObject(body);

            //Create the authentication header
            var authenticationHeader = await AzureManager.GetAuthenticationHeaderAsync().ConfigureAwait(false);

            //Execute the purge request
            var jsonResponse = await Web.WebRequestAsync(Web.HttpMethods.Post, uri, authenticationHeader, body: json, captureError:true).ConfigureAwait(false);

            //Get the operationId from the response
            if (string.IsNullOrWhiteSpace(jsonResponse)) throw new InvalidOperationException("No response returned");

            var response = JsonConvert.DeserializeObject<JObject>(jsonResponse);
            var operationId = (string)response["operationId"];
            return operationId;
        }

        public async Task<string> GetPurgeStatusAsync(string subscriptionId, string resourceGroupName, string resourceName, string purgeId)
        {
            //see https://docs.microsoft.com/en-us/rest/api/application-insights/components/getpurgestatus

            //Create the uri
            var uri = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Insights/components/{resourceName}/operations/{purgeId}?api-version={apiVersion}";

            //Create the authentication header
            var authenticationHeader = await AzureManager.GetAuthenticationHeaderAsync().ConfigureAwait(false);

            //Execute the purge request
            var jsonResponse = await Web.WebRequestAsync(Web.HttpMethods.Get, uri, authenticationHeader).ConfigureAwait(false);

            //Get the status from the response
            if (string.IsNullOrWhiteSpace(jsonResponse)) throw new InvalidOperationException("No response returned");

            var response = JsonConvert.DeserializeObject<JObject>(jsonResponse);

            var status = (string)response["status"];
            return status;
        }

        public async Task<(string subscriptionId, string resourceGroupName, string resourceName)> GetResourceInfoAsync(string instrumentationKey)
        {
            AzureManager.Authenticate();

            var appInsightsList = ListAsync(AzureManager.Azure.SubscriptionId);

            await foreach (var appInsights in appInsightsList)
            {
                if (appInsights.instrumentationKey == instrumentationKey)return (AzureManager.Azure.SubscriptionId, appInsights.resourceGroup, appInsights.resourceName);
            }
            return default;
        }
    }
}

