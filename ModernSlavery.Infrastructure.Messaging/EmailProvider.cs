using System;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.EmailTemplates;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.Core.Options;
using ModernSlavery.Infrastructure.Messaging.GovNotify;

namespace ModernSlavery.Infrastructure.Messaging
{
    public class EmailProvider : BaseEmailProvider
    {
        #region Dependencies
        private readonly GovNotifyEmailProvider _govNotifyEmailProvider;
        private readonly SmtpEmailProvider _smtpEmailProvider;
        public readonly EmailOptions EmailOptions;
        #endregion

        public EmailProvider(
            GovNotifyEmailProvider govNotifyEmailProvider,
            SmtpEmailProvider smtpEmailProvider,
            IEmailTemplateRepository emailTemplateRepo,
            EmailOptions emailOptions,
            TestOptions testOptions,
            ILogger<EmailProvider> logger,
            [KeyFilter(Filenames.EmailSendLog)] IAuditLogger emailSendLog) : base(testOptions, emailTemplateRepo, logger, emailSendLog)
        {
            _govNotifyEmailProvider = govNotifyEmailProvider ?? throw new ArgumentNullException(nameof(govNotifyEmailProvider));
            _smtpEmailProvider = smtpEmailProvider ?? throw new ArgumentNullException(nameof(smtpEmailProvider));
            EmailOptions = emailOptions ?? throw new ArgumentNullException(nameof(emailOptions));
        }

        public override async Task<SendEmailResult> SendEmailAsync<TModel>(string emailAddress, string templateId,TModel model)
        {
            SendEmailResult result=null;

            if (_govNotifyEmailProvider.Enabled)
            {
                result = await TrySendGovNotifyEmailAsync(emailAddress, templateId, model).ConfigureAwait(false);
            }

            // gov notify provider failed so trying the smtp provider
            if (result == null && _smtpEmailProvider.Enabled)
            {
                result = await TrySendSmtpEmail(emailAddress, templateId, model).ConfigureAwait(false);
            }

            return result;
        }

        private async Task<SendEmailResult> TrySendGovNotifyEmailAsync<TModel>(string emailAddress,string templateId,TModel model)
        {
            try
            {
                var result = await _govNotifyEmailProvider.SendEmailAsync(emailAddress, templateId, model).ConfigureAwait(false);
                if (!result.Status.EqualsI("created", "sending", "delivered"))throw new Exception($"Unexpected status '{result.Status}' returned from Template {templateId}");

                await EmailSendLog.WriteAsync(
                    new EmailSendLogModel
                    {
                        Message = "Email successfully sent via GovNotify",
                        Subject = result.EmailSubject,
                        Recipients = result.EmailAddress,
                        Server = result.Server
                    }).ConfigureAwait(false);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,$"{nameof(TrySendGovNotifyEmailAsync)}: Could not send email to Gov Notify Template: {templateId} using the email address: {emailAddress}");

                // send failure email to GEO using smtp email provider
                await _smtpEmailProvider.SendEmailTemplateAsync(
                    new SendEmailTemplate
                    {
                        RecipientEmailAddress = EmailOptions.AdminDistributionList,
                        Subject = "MSU - GOV NOTIFY ERROR",
                        MessageBody = $"Could not send email to Gov Notify Template:{templateId} using {emailAddress} due to following error:\n\n{ex.GetDetailsText()}.\n\nWill attempting to resend email using SMTP."
                    }).ConfigureAwait(false);
            }

            return null;
        }

        private async Task<SendEmailResult> TrySendSmtpEmail<TModel>(string emailAddress, string templateId, TModel model)
        {
            try
            {
                var result = await _smtpEmailProvider.SendEmailAsync(emailAddress, templateId, model).ConfigureAwait(false);

                await EmailSendLog.WriteAsync(
                    new EmailSendLogModel
                    {
                        Message = "Email successfully sent via SMTP",
                        Subject = result.EmailSubject,
                        Recipients = result.EmailAddress,
                        Server = result.Server
                    }).ConfigureAwait(false);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    ex,
                    "{FuncName}: Cannot send email to {Email} using SMTP. TemplateId: {TemplateId}",
                    nameof(TrySendSmtpEmail),
                    emailAddress,
                    templateId);
            }

            return null;
        }
    }
}