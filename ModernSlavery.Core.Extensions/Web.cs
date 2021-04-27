using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Polly;
using Polly.Extensions.Http;

namespace ModernSlavery.Core.Extensions
{
    public static class Web
    {
        public enum HttpMethods : byte
        {
            Get,
            Post,
            Delete,
            Patch,
            Put
        }

        public static async Task<string> WebRequestAsync(HttpMethods httpMethod,
        string url,
        string username = null,
        string password = null,
        string body = null,
            Dictionary<string, string> headers = null, bool validateCertificate=true,
            int timeOut = 0, bool captureError = false)
        {
            var authenticationHeader = string.IsNullOrWhiteSpace(username) &&  string.IsNullOrWhiteSpace(password) ? null : new AuthenticationHeaderValue("Basic",Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));
            return await WebRequestAsync(httpMethod, url, authenticationHeader, headers, body, validateCertificate, captureError).ConfigureAwait(false);
        }

        public static async Task<string> WebRequestAsync(HttpMethods httpMethod,
            string url, AuthenticationHeaderValue authenticationHeader,
            Dictionary<string, string> headers = null, string body = null, bool validateCertificate=true, bool captureError = false)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
                if (!validateCertificate)httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                using (var client = new HttpClient(httpClientHandler))
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    if (authenticationHeader!=null)client.DefaultRequestHeaders.Authorization = authenticationHeader;

                    if (headers != null)
                        foreach (var key in headers.Keys)
                            client.DefaultRequestHeaders.Add(key, headers[key]);

                    HttpContent httpContent = null;
                    if (body != null)
                    {
                        if (!httpMethod.IsAny(HttpMethods.Post, HttpMethods.Put, HttpMethods.Patch))
                            throw new ArgumentOutOfRangeException(
                                nameof(httpMethod),
                                "HttpMethod must be Post, Put or Patch when a body is specified");

                        if (string.IsNullOrWhiteSpace(body)) throw new ArgumentNullException("body", "body is empty");

                        httpContent = new StringContent(
                            body,
                            Encoding.UTF8,
                            httpMethod == HttpMethods.Patch ? "application/json-patch+json" : "application/json");
                    }
                    else if (httpMethod.IsAny(HttpMethods.Post, HttpMethods.Put, HttpMethods.Patch) &&
                             (headers == null || headers.Count == 0))
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(body),
                            "You must supply a body (or additional headers) when Post, Put or Patch when a body is specified");
                    }


                    using (var response = httpMethod == HttpMethods.Get ? await client.GetAsync(url).ConfigureAwait(false) :
                        httpMethod == HttpMethods.Delete ? await client.DeleteAsync(url).ConfigureAwait(false) :
                        httpMethod == HttpMethods.Post ? await client.PostAsync(url, httpContent).ConfigureAwait(false) :
                        httpMethod == HttpMethods.Put ? await client.PutAsync(url, httpContent).ConfigureAwait(false) :
                        httpMethod == HttpMethods.Patch ? await client.PatchAsync(url, httpContent).ConfigureAwait(false) :
                        throw new ArgumentOutOfRangeException(nameof(httpMethod),
                            "HttpMethod must be Get, Delete, Post or Put"))
                    {
                        string responseBody = null;
                        try
                        {
                            response.EnsureSuccessStatusCode();
                        }
                        catch (Exception ex)
                        {
                            if (captureError)
                            {
                                responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                if (!string.IsNullOrWhiteSpace(responseBody))
                                    throw new Exception(responseBody, ex);
                            }
                            throw ex;
                        }
                        responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        if (headers != null)
                            foreach (var header in response.Headers)
                                headers[header.Key] = header.Value.Distinct().ToDelimitedString();

                        return responseBody;
                    }
                }
            }
        }


        public static HttpClient SetupHttpClient(this HttpClient httpClient, string baseUrl=null, int connectionLeaseTimeoutSeconds = 60, int timeoutSeconds=100)
        {
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                httpClient.BaseAddress = new Uri(baseUrl);
                ServicePointManager.FindServicePoint(httpClient.BaseAddress).ConnectionLeaseTimeout = connectionLeaseTimeoutSeconds * 1000;
            }

            httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.ConnectionClose = false;
            return httpClient;
        }

        public static HttpClientHandler ConfigureHttpMessageHandler(bool acceptBadCertificate=true, int maxAutomaticRedirections=50, DecompressionMethods automaticDecompression = DecompressionMethods.All)
        {
            var httpClientHandler = new HttpClientHandler
            {
                //Alow automatic redirection up to 10 hops
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = maxAutomaticRedirections,

                //This is required for being able to decompress
                AutomaticDecompression = automaticDecompression, 
            };

            //Allow invalid certificates
            if (acceptBadCertificate) httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            return httpClientHandler;
        }

        public static HttpClient AddBrowserHeaders(this HttpClient client)
        {
            //Accept headers required to preventabort
            client.DefaultRequestHeaders.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("accept-language", "en-GB,en-US;q=0.9,en;q=0.8");

            //User agent is required to prevent 403 forbidden
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36");

            return client;
        }

    }
}