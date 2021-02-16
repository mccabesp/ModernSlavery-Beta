using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

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

        
    }
}