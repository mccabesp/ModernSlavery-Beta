using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;

namespace ModernSlavery.Infrastructure.CompaniesHouse
{
    public class PostcodeChecker : IPostcodeChecker
    {
        private readonly PostcodeCheckerOptions _postcodeCheckerOptions;
        private readonly HttpClient _httpClient;

        public PostcodeChecker(PostcodeCheckerOptions postcodeCheckerOptions, HttpClient httpClient)
        {
            _postcodeCheckerOptions = postcodeCheckerOptions ?? throw new ArgumentNullException(nameof(postcodeCheckerOptions));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<bool> CheckPostcodeAsync(string postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode)) throw new ArgumentNullException(nameof(postcode));

            var response = await _httpClient.GetAsync($"/postcodes/{postcode}/validate").ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var bodyString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var body = JsonConvert.DeserializeObject<PostcodesIoApiValidateResponse>(bodyString);
            return body.result;
        }

        public static void SetupHttpClient(HttpClient httpClient, string apiServer)
        {
            httpClient.BaseAddress = new Uri(apiServer);

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.ConnectionClose = false;
            ServicePointManager.FindServicePoint(httpClient.BaseAddress).ConnectionLeaseTimeout = 60 * 1000;
        }

        public static IAsyncPolicy<HttpResponseMessage> GetLinearRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(10,retryAttempt => TimeSpan.FromMilliseconds(new Random().Next(100, 1000)));
        }
        public static IAsyncPolicy<HttpResponseMessage> GetExponentialRetryPolicy()
        {
            var jitterer = new Random();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))+TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)));
        }
    }

    internal class PostcodesIoApiValidateResponse
    {
        public bool result { get; set; }
    }

    
}