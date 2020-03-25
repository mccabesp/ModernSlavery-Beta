using System;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Options;
using Notify.Client;
using Notify.Interfaces;

namespace ModernSlavery.Infrastructure.Messaging
{
    public class GovNotifyEmailProvider : BaseEmailProvider
    {
        public GovNotifyEmailProvider(
            IHttpClientFactory httpClientFactory,
            IEmailTemplateRepository emailTemplateRepo,
            GovNotifyOptions govNotifyOptions,
            SharedOptions sharedOptions,
            ILogger<GovNotifyEmailProvider> logger,
            [KeyFilter(Filenames.EmailSendLog)] IAuditLogger emailSendLog)
            : base(sharedOptions, emailTemplateRepo, logger, emailSendLog)
        {
            Options = govNotifyOptions
                      ?? throw new ArgumentNullException("You must provide the gov notify email options",
                          nameof(govNotifyOptions));

            if (Enabled)
            {
                // ensure we have api keys
                if (string.IsNullOrWhiteSpace(Options.ApiKey))
                    throw new NullReferenceException(
                        $"{nameof(Options.ApiKey)}: You must supply a production api key for GovNotify");

                if (string.IsNullOrWhiteSpace(Options.ApiTestKey))
                    throw new NullReferenceException(
                        $"{nameof(Options.ApiTestKey)}: You must supply a test api key for GovNotify");
            }

            // create the clients
            var httpClient = httpClientFactory.CreateClient(nameof(GovNotifyEmailProvider));
            var notifyHttpWrapper = new HttpClientWrapper(httpClient);
            ProductionClient = new NotificationClient(notifyHttpWrapper, Options.ApiKey);
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
            var client = test ? TestClient : ProductionClient;

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
    }
}