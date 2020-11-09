using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.BusinessDomain.Viewing
{
    public interface IUrlChecker
    {
        public Task<bool> IsUrlWorking(string url);
    }

    public class UrlChecker : IUrlChecker
    {
        IHttpClientFactory ClientFactory;

        public UrlChecker(IHttpClientFactory clientFactory)
        {
            ClientFactory = clientFactory;
        }

        public async Task<bool> IsUrlWorking(string url)
        {
            Uri uri;

            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                return false;

            var request = new HttpRequestMessage(HttpMethod.Head, uri);
            var client = ClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

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
                return false;
            }
        }
    }
}
