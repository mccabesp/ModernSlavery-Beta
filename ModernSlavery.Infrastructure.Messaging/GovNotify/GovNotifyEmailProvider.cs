using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Messaging.GovNotify
{
    public class GovNotifyEmailProvider : BaseEmailProvider
    {
        private readonly IGovNotifyAPI _govNotifyAPI;
        private readonly GovNotifyOptions _govNotifyEmailOptions;

        public GovNotifyEmailProvider(
            IGovNotifyAPI govNotifyAPI,
            IEmailTemplateRepository emailTemplateRepo,
            GovNotifyOptions govNotifyEmailOptions,
            TestOptions testOptions,
            ILogger<GovNotifyEmailProvider> logger,
            [KeyFilter(Filenames.EmailSendLog)] IAuditLogger emailSendLog) : base(testOptions, emailTemplateRepo, logger, emailSendLog)
        {
            _govNotifyAPI = govNotifyAPI;
            _govNotifyEmailOptions = govNotifyEmailOptions;
        }

        public override bool Enabled => _govNotifyEmailOptions.Enabled != false;

        public override async Task<SendEmailResult> SendEmailAsync<TTemplate>(string emailAddress, string templateId, TTemplate parameters)
        {
            var sendRequest = new SendEmailRequest { EmailAddress = emailAddress, TemplateId = templateId, Personalisation = parameters.GetPropertiesDictionary() };

            // send email
            var response = await _govNotifyAPI.SendEmailAsync(sendRequest).ConfigureAwait(false);

            // get result
            var result = await _govNotifyAPI.GetEmailResultAsync(response.EmailId).ConfigureAwait(false);

            return result;
        }
    }
}