using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using ModernSlavery.Core.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.Core.Classes
{
    public interface IUrlChecker
    {
        public Task<bool> IsUrlWorking(string url);
    }

    public class UrlChecker : IUrlChecker
    {
        readonly ILogger<UrlChecker> Logger;
        readonly UrlCheckerOptions Options;
        readonly IHttpClientFactory ClientFactory;

        public UrlChecker(
            ILogger<UrlChecker> logger,
            UrlCheckerOptions options,
            IHttpClientFactory clientFactory)
        {
            ClientFactory = clientFactory;
            Logger = logger;
            Options = options;
        }

        public async Task<bool> IsUrlWorking(string url)
        {
            Uri uri;

            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                return false;

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var client = ClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(Options.Timeout);

            try
            {
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                // exception due to timeout or DNS error
                Logger.LogError(ex, "Failed loading {url} when checking exists", url);
                return false;
            }
        }
    }
}
