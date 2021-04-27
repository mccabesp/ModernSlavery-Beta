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
    }

    internal class PostcodesIoApiValidateResponse
    {
        public bool result { get; set; }
    }

    
}