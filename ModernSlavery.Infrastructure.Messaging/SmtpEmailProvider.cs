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
        public SmtpEmailProvider(IEmailTemplateRepository emailTemplateRepo,
            SmtpEmailOptions smtpEmailOptions,
            TestOptions testOptions,
            ILogger<SmtpEmailProvider> logger,
            [KeyFilter(Filenames.EmailSendLog)] IAuditLogger emailSendLog)
            : base(testOptions, emailTemplateRepo, logger, emailSendLog)
        {
            Options = smtpEmailOptions ?? throw new ArgumentNullException(nameof(smtpEmailOptions));
            //TODO ensure smtp config is present (when enabled)
        }

        public SmtpEmailOptions Options { get; }

        public override bool Enabled => Options.Enabled != false;

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
            if (templateMetaData == null) new NullReferenceException(nameof(templateMetaData));

            templateMetaData.Remove();

            var messageSubject = emailTemplateInfo.EmailSubject;
            var messageHtml = document.ToHtml();
            var messageText = document.Text();

            // merge the template parameters
            foreach ((string name, object value) in mergeParameters)
            {
                messageSubject = messageSubject.Replace($"(({name}))", value.ToString());
                messageHtml = messageHtml.Replace($"(({name}))", value.ToString());
            }

            await Email.QuickSendAsync(
                messageSubject,
                Options.SenderEmail,
                Options.SenderName,
                Options.ReplyEmail,
                emailAddress,
                messageHtml,
                Options.Server,
                Options.Username,
                Options.Password,
                Options.Port,
                simulate: _testOptions.SimulateMessageSend);

            return new SendEmailResult
            {
                Status = "sent",
                Server = $"{Options.Server}:{Options.Port}",
                ServerUsername = Options.Username,
                EmailAddress = emailAddress,
                EmailSubject = messageSubject,
                EmailMessagePlainText = messageText
            };
        }
    }
}