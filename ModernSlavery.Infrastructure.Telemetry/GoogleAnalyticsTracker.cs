using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.Infrastructure.Telemetry
{
    /// <summary>
    ///     Uses open HttpClient see
    ///     https://www.codeproject.com/Articles/1194406/Using-HttpClient-as-it-was-intended-because-you-re
    /// </summary>
    public class GoogleAnalyticsTracker : IWebTracker, IDisposable
    {
        public static Uri BaseUri = new Uri("https://www.google-analytics.com");
        private static readonly Uri endpointUri = new Uri(BaseUri, "/collect");

        private readonly HttpClient _httpClient;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GoogleAnalyticsTracker> _Logger;
        private readonly string googleTrackingId; // UA-XXXXXXXXX-XX

        private readonly string googleVersion = "1";

        public GoogleAnalyticsTracker(ILogger<GoogleAnalyticsTracker> logger, IHttpContextAccessor httpContextAccessor,
            HttpClient httpClient, string trackingId)
        {
            _Logger = logger;
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            googleTrackingId = trackingId;
        }

        private string googleClientId =>
            _httpContextAccessor.HttpContext?.Session?.Id ?? Guid.NewGuid().ToString(); // 555 - any user identifier

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        public async Task SendPageViewTrackingAsync(string title, string url)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentNullException(nameof(title));

            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));

            if (!url.IsUrl()) throw new ArgumentException("Url is not absolute", nameof(url));

            var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("v", googleVersion),
                new KeyValuePair<string, string>("tid", googleTrackingId),
                new KeyValuePair<string, string>("cid", googleClientId),
                new KeyValuePair<string, string>("t", "pageview"),
                new KeyValuePair<string, string>("dt", title),
                new KeyValuePair<string, string>("dl", url)
            };

            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.PostAsync(endpointUri, new FormUrlEncodedContent(postData))
                    .ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, $"Could not track title:'{title}', url:'{url}'");
            }
        }
    }
}