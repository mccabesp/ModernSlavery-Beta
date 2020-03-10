using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Extensions;
using ModernSlavery.Extensions.AspNetCore;
using ModernSlavery.SharedKernel;

namespace ModernSlavery.Infrastructure.Message
{

    public class SmtpEmailProvider : AEmailProvider
    {

        public SmtpEmailProvider(IEmailTemplateRepository emailTemplateRepo,
            IOptions<SmtpEmailOptions> smtpEmailOptions,
            ILogger<SmtpEmailProvider> logger,
            [KeyFilter(Filenames.EmailSendLog)]ILogRecordLogger emailSendLog)
            : base(emailTemplateRepo, logger, emailSendLog)
        {
            Options = smtpEmailOptions ?? throw new ArgumentNullException(nameof(smtpEmailOptions));
            //TODO ensure smtp config is present (when enabled)
        }

        public IOptions<SmtpEmailOptions> Options { get; }

        public override bool Enabled => Options.Value.Enabled != false;

        public override async Task<SendEmailResult> SendEmailAsync<TModel>(string emailAddress, string templateId, TModel model, bool test)
        {
            // convert the model's public properties to a dictionary
            Dictionary<string, object> mergeParameters = model.GetPropertiesDictionary();

            // prefix subject with environment name
            mergeParameters["Environment"] = Config.IsProduction() ? "" : $"[{Config.EnvironmentName}] ";

            // get template
            EmailTemplateInfo emailTemplateInfo = EmailTemplateRepo.GetByTemplateId(templateId);
            string htmlContent = System.IO.File.ReadAllText(emailTemplateInfo.FilePath);

            // parse html
            var parser = new HtmlParser();
            IHtmlDocument document = parser.ParseDocument(htmlContent);

            // remove the meta data comments from the document
            IComment templateMetaData = document.Descendents<IComment>().FirstOrDefault();
            if (templateMetaData == null)
            {
                new NullReferenceException(nameof(templateMetaData));
            }

            templateMetaData.Remove();

            string messageSubject = emailTemplateInfo.EmailSubject;
            string messageHtml = document.ToHtml();
            string messageText = document.Text();

            // merge the template parameters
            foreach ((string name, object value) in mergeParameters)
            {
                messageSubject = messageSubject.Replace($"(({name}))", value.ToString());
                messageHtml = messageHtml.Replace($"(({name}))", value.ToString());
            }

            string smtpServer = string.IsNullOrWhiteSpace(Options.Value.Server) ? Options.Value.Server2 : Options.Value.Server;
            int smtpServerPort = (string.IsNullOrWhiteSpace(Options.Value.Port) ? Options.Value.Port2: Options.Value.Port).ToInt32(25);
            string smtpUsername = string.IsNullOrWhiteSpace(Options.Value.Username) ? Options.Value.Username2 : Options.Value.Username;
            string smtpPassword = string.IsNullOrWhiteSpace(Options.Value.Password) ? Options.Value.Password2 : Options.Value.Password;

            await Email.QuickSendAsync(
                messageSubject,
                Options.Value.SenderEmail,
                Options.Value.SenderName,
                Options.Value.ReplyEmail,
                emailAddress,
                messageHtml,
                smtpServer,
                smtpUsername,
                smtpPassword,
                smtpServerPort,
                test: test);

            return new SendEmailResult {
                Status = "sent",
                Server = $"{smtpServer}:{smtpServerPort}",
                ServerUsername = smtpUsername,
                EmailAddress = emailAddress,
                EmailSubject = messageSubject,
                EmailMessagePlainText = messageText
            };
        }

    }

}
