using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using Notify.Client;
using Notify.Interfaces;
using Polly;
using Polly.Extensions.Http;

namespace ModernSlavery.Infrastructure.Messaging
{
    public class GovNotifyEmailProvider : BaseEmailProvider
    {
        public GovNotifyEmailProvider(
            HttpClient httpClient,
            IEmailTemplateRepository emailTemplateRepo,
            GovNotifyOptions govNotifyOptions,
            SharedOptions sharedOptions,
            ILogger<GovNotifyEmailProvider> logger,
            [KeyFilter(Filenames.EmailSendLog)] IAuditLogger emailSendLog
            )
            : base(sharedOptions, emailTemplateRepo, logger, emailSendLog)
        {
            Options = govNotifyOptions
                      ?? throw new ArgumentNullException("You must provide the gov notify email options",
                          nameof(govNotifyOptions));

            if (Enabled)
            {
                // ensure we have api keys
                if (!Options.AllowTestKeyOnly && string.IsNullOrWhiteSpace(Options.ApiKey))
                    throw new NullReferenceException(
                        $"{nameof(Options.ApiKey)}: You must supply a production api key for GovNotify");

                if (string.IsNullOrWhiteSpace(Options.ApiTestKey))
                    throw new NullReferenceException(
                        $"{nameof(Options.ApiTestKey)}: You must supply a test api key for GovNotify");
                else if (!Options.ApiTestKey.StartsWith("test_only-"))
                    throw new ArgumentException("GovNotify Api Test Key must start with 'test_only-'", nameof(Options.ApiTestKey));
            }
            
            // create the clients
            var notifyHttpWrapper = new HttpClientWrapper(httpClient);
            if (!Options.AllowTestKeyOnly) ProductionClient = new NotificationClient(notifyHttpWrapper, Options.ApiKey);
            TestClient = new NotificationClient(notifyHttpWrapper, Options.ApiTestKey);
        }

        public override bool Enabled => Options.Enabled != false;

        public override async Task<SendEmailResult> SendEmailAsync<TTemplate>(string emailAddress,
            string templateId,
            TTemplate parameters,
            bool test)
        {
            // convert the model's public properties to a dictionary
            var mergeParameters = parameters.GetPropertiesDictionary();

            // prefix subject with environment name
            mergeParameters["Environment"] = SharedOptions.IsProduction() ? "" : $"[{SharedOptions.Environment}] ";

            // determine which client to use
            var client = test || Options.AllowTestKeyOnly ? TestClient : ProductionClient;

            // send email
            var response = await client.SendEmailAsync(
                emailAddress,
                templateId,
                mergeParameters,
                Options.ClientReference);

            // get result
            var notification = await client.GetNotificationByIdAsync(response.id);

            return new SendEmailResult
            {
                Status = notification.status,
                Server = "Gov Notify",
                ServerUsername = Options.ClientReference,
                EmailAddress = emailAddress,
                EmailSubject = notification.subject
            };
        }

        #region Dependencies

        public IAsyncNotificationClient ProductionClient { get; }

        public IAsyncNotificationClient TestClient { get; }

        public GovNotifyOptions Options { get; }

        #endregion

        public static void SetupHttpClient(HttpClient httpClient, string apiServer)
        {
            httpClient.BaseAddress = new Uri(apiServer);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.ConnectionClose = false;
            ServicePointManager.FindServicePoint(httpClient.BaseAddress).ConnectionLeaseTimeout = 60 * 1000;
        }

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt =>
                        TimeSpan.FromMilliseconds(new Random().Next(1, 1000)) +
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}