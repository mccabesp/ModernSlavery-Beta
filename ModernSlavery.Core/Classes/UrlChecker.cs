using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.Core.Classes
{
    public interface IUrlChecker
    {
        public Task<bool> IsUrlWorkingAsync(string url);
    }

    public class UrlChecker : IUrlChecker
    {
        private readonly ILogger<UrlChecker> _logger;
        private readonly UrlCheckerOptions _options;

        public UrlChecker(ILogger<UrlChecker> logger,UrlCheckerOptions options)
        {
            _logger = logger;
            _options = options;
        }

        public async Task<bool> IsUrlWorkingAsync(string url)
        {
            if (_options.Disabled) return true;

            using var httpClientHandler = Web.ConfigureHttpMessageHandler();
            using var httpClient = new HttpClient(httpClientHandler,true);
            Web.SetupHttpClient(httpClient).AddBrowserHeaders();

            var retryPolicy = _options.RetryPolicy == RetryPolicyTypes.Exponential ? Resilience.GetExponentialAsyncRetryPolicy(3) : Resilience.GetLinearAsyncRetryPolicy(3);

            //Try a http HEAD first as this is the fastest
            var headResponseStatus = "Unknown";
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, url);

                //Make the request using Polly retry policy
                using var headResponse = await retryPolicy.ExecuteAsync(async () => await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false)).ConfigureAwait(false);

                if (headResponse.StatusCode == System.Net.HttpStatusCode.OK) return true;
                headResponseStatus = headResponse.StatusCode.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Url: {url} HEAD request failed");
                headResponseStatus = ex.Message;
            }


            //Then try a http GET first as this is the fastest
            try
            {
                //Make the request using Polly retry policy
                var getResponse = await retryPolicy.ExecuteAsync(async () => await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false)).ConfigureAwait(false);
               
                if (getResponse.StatusCode == System.Net.HttpStatusCode.OK) return true;

                throw new HttpOperationException($"Invalid HTTP response HEAD({headResponseStatus}) and GET({getResponse.StatusCode}) - expected 200 OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Url: {url} GET check failed");
            }
            return false;
        }

    }
}
