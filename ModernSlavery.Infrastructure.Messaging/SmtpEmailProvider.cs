using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Messaging
{
    public class SmtpEmailProvider : BaseEmailProvider
    {
        private SmtpEmailOptions _smtpEmailOptions { get; }
        public SmtpEmailProvider(IEmailTemplateRepository emailTemplateRepo,
            SmtpEmailOptions smtpEmailOptions,
            TestOptions testOptions,
            ILogger<SmtpEmailProvider> logger,
            [KeyFilter(Filenames.EmailSendLog)] IAuditLogger emailSendLog)
            : base(testOptions, emailTemplateRepo, logger, emailSendLog)
        {
            _smtpEmailOptions = smtpEmailOptions ?? throw new ArgumentNullException(nameof(smtpEmailOptions));
            //TODO ensure smtp config is present (when enabled)
        }


        public override bool Enabled => _smtpEmailOptions.Enabled != false;

        public override async Task<SendEmailResult> SendEmailAsync<TModel>(string emailAddress, string templateId, TModel model)
        {
            // convert the model's public properties to a dictionary
            var mergeParameters = model.GetPropertiesDictionary();

            // prefix subject with environment name
            mergeParameters["Environment"] = _testOptions.IsProduction() ? "" : $"[{_testOptions.Environment}] ";

            // get template
            var emailTemplateInfo = EmailTemplateRepo.GetByTemplateId(templateId);
            var htmlContent = File.ReadAllText(emailTemplateInfo.FilePath);

            // parse html
            var parser = new HtmlParser();
            var document = parser.ParseDocument(htmlContent);

            // remove the meta data comments from the document
            var templateMetaData = document.Descendents<IComment>().FirstOrDefault();
            if (templateMetaData == null) throw new NullReferenceException(nameof(templateMetaData));

            templateMetaData.Remove();

            var messageSubject = emailTemplateInfo.EmailSubject;
            var messageHtml = document.ToHtml();
            var messageText = document.Text();

            // merge the template parameters
            foreach ((string name, object value) in mergeParameters)
            {
                messageSubject = messageSubject.Replace($"(({name}))", value?.ToString(), StringComparison.OrdinalIgnoreCase);
                messageHtml = messageHtml.Replace($"(({name}))", value?.ToString(),StringComparison.OrdinalIgnoreCase);
            }

            await Email.QuickSendAsync(
                messageSubject,
                _smtpEmailOptions.SenderEmail,
                _smtpEmailOptions.SenderName,
                _smtpEmailOptions.ReplyEmail,
                emailAddress,
                messageHtml,
                _smtpEmailOptions.Server,
                _smtpEmailOptions.Username,
                _smtpEmailOptions.Password,
                _smtpEmailOptions.Port,
                simulate: _testOptions.SimulateMessageSend).ConfigureAwait(false);

            return new SendEmailResult
            {
                Status = "sent",
                Server = $"{_smtpEmailOptions.Server}:{_smtpEmailOptions.Port}",
                ServerUsername = _smtpEmailOptions.Username,
                EmailAddress = emailAddress,
                EmailSubject = messageSubject,
                EmailMessagePlainText = messageText
            };
        }
    }
}