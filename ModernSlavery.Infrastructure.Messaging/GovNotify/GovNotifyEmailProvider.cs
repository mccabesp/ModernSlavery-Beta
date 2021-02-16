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
using ModernSlavery.Core.Options;
using Notify.Client;
using Notify.Interfaces;
using Polly;
using Polly.Extensions.Http;

namespace ModernSlavery.Infrastructure.Messaging.GovNotify
{
    public class GovNotifyEmailProvider : BaseEmailProvider
    {
        private readonly IGovNotifyAPI _govNotifyAPI;

        public GovNotifyEmailProvider(
            IGovNotifyAPI govNotifyAPI,
            IEmailTemplateRepository emailTemplateRepo,
            TestOptions testOptions,
            ILogger<GovNotifyEmailProvider> logger,
            [KeyFilter(Filenames.EmailSendLog)] IAuditLogger emailSendLog) : base(testOptions, emailTemplateRepo, logger, emailSendLog)
        {
            _govNotifyAPI = govNotifyAPI;
        }

        public override async Task<SendEmailResult> SendEmailAsync<TTemplate>(string emailAddress, string templateId, TTemplate parameters)
        {
            var sendRequest = new SendEmailRequest { EmailAddress = emailAddress, TemplateId = templateId, Personalisation = parameters.GetPropertiesDictionary() };

            // send email
            var response = await _govNotifyAPI.SendEmailAsync(sendRequest).ConfigureAwait(false);

            // get result
            return await _govNotifyAPI.GetEmailResultAsync(response.EmailId).ConfigureAwait(false);
        }
    }
}