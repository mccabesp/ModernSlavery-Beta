using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;
        private readonly IEventLogger _customLogger;

        private readonly GovNotifyOptions _govNotifyOptions;
        private readonly TestOptions _testOptions;
        public bool Enabled => _govNotifyOptions.Enabled != false;

        public GovNotifyAPI(HttpClient httpClient, GovNotifyOptions govNotifyOptions, TestOptions testOptions, ILogger<GovNotifyAPI> logger, IEventLogger customLogger)
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

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customLogger = customLogger ?? throw new ArgumentNullException(nameof(customLogger));
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
                    sendEmailRequest.Personalisation, _govNotifyOptions.ClientReference).ConfigureAwait(false);

                return response != null ? new SendEmailResponse { EmailId = response.id } : throw new NotifyClientException("No response returned");
            }
            catch (NotifyClientException e)
            {
                _customLogger.Error(
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
            var response = await _govNotifyClient.SendLetterAsync(templateId, personalisation, clientReference).ConfigureAwait(false);
            return response != null ? new SendLetterResponse { LetterId = response.id } : throw new NotifyClientException("No response returned");
        }

        public async Task<SendEmailResult> GetEmailResultAsync(string emailId)
        {
            var notification = await _govNotifyClient.GetNotificationByIdAsync(emailId).ConfigureAwait(false);

            return new SendEmailResult
            {
                Status = notification.status,
                Server = "Gov Notify",
                ServerUsername = _govNotifyOptions.ClientReference,
                EmailAddress = notification.emailAddress,
                EmailSubject = notification.subject
            };
        }
    }
}