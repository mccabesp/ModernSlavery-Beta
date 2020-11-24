using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Exceptions;
using Notify.Interfaces;
using Polly;
using Polly.Extensions.Http;

namespace ModernSlavery.Infrastructure.Messaging.GovNotify
{
    public class GovNotifyAPI : IGovNotifyAPI
    {
        private readonly IAsyncNotificationClient _govNotifyClient;
        private readonly IEventLogger CustomLogger;

        private readonly GovNotifyOptions _govNotifyOptions;
        private readonly TestOptions _testOptions;
        public bool Enabled => _govNotifyOptions.Enabled != false;

        public GovNotifyAPI(HttpClient httpClient, GovNotifyOptions govNotifyOptions, TestOptions testOptions, IEventLogger customLogger)
        {
            _govNotifyOptions = govNotifyOptions ?? throw new ArgumentNullException(nameof(govNotifyOptions));
            _testOptions = testOptions ?? throw new ArgumentNullException(nameof(testOptions));

            if (Enabled)
            {
                // ensure we have api keys
                if (!testOptions.SimulateMessageSend && string.IsNullOrWhiteSpace(_govNotifyOptions.ApiKey))
                    throw new NullReferenceException(
                        $"{nameof(_govNotifyOptions.ApiKey)}: You must supply a production api key for GovNotify");

                if (string.IsNullOrWhiteSpace(_govNotifyOptions.ApiTestKey))
                    throw new NullReferenceException($"{nameof(_govNotifyOptions.ApiTestKey)}: You must supply a test api key for GovNotify");

                if (!_govNotifyOptions.ApiTestKey.StartsWith("test_only-"))
                    throw new ArgumentException("GovNotify Api Test Key must start with 'test_only-'", nameof(_govNotifyOptions.ApiTestKey));
            }

            // create the clients
            var notifyHttpWrapper = new HttpClientWrapper(httpClient);
            _govNotifyClient = _testOptions.SimulateMessageSend ? new NotificationClient(notifyHttpWrapper, _govNotifyOptions.ApiTestKey) : new NotificationClient(notifyHttpWrapper, _govNotifyOptions.ApiKey);

            CustomLogger = customLogger ?? throw new ArgumentNullException(nameof(customLogger));
        }

        public async Task<SendEmailResponse> SendEmailAsync(SendEmailRequest sendEmailRequest)
        {
            // prefix subject with environment name
            sendEmailRequest.Personalisation["Environment"] = _testOptions.IsProduction() ? "" : $"[{_testOptions.Environment}] ";

            try
            {
                var response = await _govNotifyClient.SendEmailAsync(
                    sendEmailRequest.EmailAddress,
                    sendEmailRequest.TemplateId,
                    sendEmailRequest.Personalisation, _govNotifyOptions.ClientReference);

                return response != null ? new SendEmailResponse { EmailId = response.id } : throw new NotifyClientException("No response returned");
            }
            catch (NotifyClientException e)
            {
                CustomLogger.Error(
                    "EMAIL FAILURE: Error whilst sending email using Gov.UK Notify",
                    new
                    {
                        NotifyEmail = sendEmailRequest,
                        Personalisation = JsonConvert.SerializeObject(sendEmailRequest.Personalisation),
                        ErrorMessageFromNotify = e.Message
                    });
                throw;
            }
        }

        public async Task<SendLetterResponse> SendLetterAsync(string templateId, Dictionary<string, dynamic> personalisation, string clientReference = null)
        {
            try
            {
                var response = await _govNotifyClient.SendLetterAsync(templateId, personalisation, clientReference);
                return response != null ? new SendLetterResponse { LetterId = response.id } : throw new NotifyClientException("No response returned");
            }
            catch (NotifyClientException e)
            {
                CustomLogger.Error(
                    "Error whilst sending letter using Gov.UK Notify",
                    new
                    {
                        NotifyTemplateId = templateId,
                        Personalisation = JsonConvert.SerializeObject(personalisation),
                        ErrorMessageFromNotify = e.Message
                    });

                return null;
            }
        }

        public async Task<SendEmailResult> GetEmailResultAsync(string emailId)
        {
            var notification = await _govNotifyClient.GetNotificationByIdAsync(emailId);

            return new SendEmailResult
            {
                Status = notification.status,
                Server = "Gov Notify",
                ServerUsername = _govNotifyOptions.ClientReference,
                EmailAddress = notification.emailAddress,
                EmailSubject = notification.subject
            };
        }

        public static void SetupHttpClient(HttpClient httpClient, string apiServer)
        {
            httpClient.BaseAddress = new Uri(apiServer);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.ConnectionClose = false;
            ServicePointManager.FindServicePoint(httpClient.BaseAddress).ConnectionLeaseTimeout = 60 * 1000;
        }

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(bool linear)
        {
            return linear
                    ? HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(new Random().Next(100, 500)))
                    : HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromMilliseconds(new Random().Next(100, 1000)) + TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}