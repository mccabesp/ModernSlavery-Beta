using System;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.EmailTemplates;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.Core.Options;
using ModernSlavery.Extensions;
using ModernSlavery.SharedKernel;
using ModernSlavery.SharedKernel.Options;

namespace ModernSlavery.Infrastructure.Message
{
    public class EmailProvider : BaseEmailProvider
    {
        public EmailProvider(
            GovNotifyEmailProvider govNotifyEmailProvider,
            SmtpEmailProvider smtpEmailProvider,
            IEmailTemplateRepository emailTemplateRepo,
            EmailOptions emailOptions,
            GlobalOptions globalOptions,
            ILogger<EmailProvider> logger,
            [KeyFilter(Filenames.EmailSendLog)] ILogRecordLogger emailSendLog) : base(globalOptions, emailTemplateRepo,
            logger, emailSendLog)
        {
            GovNotifyEmailProvider =
                govNotifyEmailProvider ?? throw new ArgumentNullException(nameof(govNotifyEmailProvider));
            SmtpEmailProvider = smtpEmailProvider ?? throw new ArgumentNullException(nameof(smtpEmailProvider));
            EmailOptions = emailOptions ?? throw new ArgumentNullException(nameof(emailOptions));
        }

        public override async Task<SendEmailResult> SendEmailAsync<TModel>(string emailAddress, string templateId,
            TModel model, bool test)
        {
            SendEmailResult result;

            if (GovNotifyEmailProvider.Enabled)
            {
                result = await TrySendGovNotifyEmailAsync(emailAddress, templateId, model, test);

                if (result != null)
                    // gov notify provider succeeded 
                    return result;
            }

            // gov notify provider failed so trying the smtp provider
            result = await TrySendSmtpEmail(emailAddress, templateId, model, test);

            return result;
        }

        private async Task<SendEmailResult> TrySendGovNotifyEmailAsync<TModel>(string emailAddress,
            string templateId,
            TModel model,
            bool test)
        {
            try
            {
                var result = await GovNotifyEmailProvider.SendEmailAsync(emailAddress, templateId, model, test);
                if (result.Status.EqualsI("created", "sending", "delivered") == false)
                    throw new Exception($"Unexpected status '{result.Status}' returned");

                await EmailSendLog.WriteAsync(
                    new EmailSendLogModel
                    {
                        Message = "Email successfully sent via GovNotify",
                        Subject = result.EmailSubject,
                        Recipients = result.EmailAddress,
                        Server = result.Server
                    });

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    ex,
                    "{FuncName}: Could not send email to Gov Notify using the email address: {Email}:",
                    nameof(TrySendGovNotifyEmailAsync),
                    emailAddress);

                // send failure email to GEO using smtp email provider
                await SmtpEmailProvider.SendEmailTemplateAsync(
                    new SendEmailTemplate
                    {
                        RecipientEmailAddress = EmailOptions.GEODistributionList,
                        Subject = "GPG - GOV NOTIFY ERROR",
                        MessageBody =
                            $"Could not send email to Gov Notify using {emailAddress} due to following error:\n\n{ex.GetDetailsText()}.\n\nWill attempting to resend email using SMTP."
                    });
            }

            return null;
        }

        private async Task<SendEmailResult> TrySendSmtpEmail<TModel>(string emailAddress, string templateId,
            TModel model, bool test)
        {
            try
            {
                var result = await SmtpEmailProvider.SendEmailAsync(emailAddress, templateId, model, test);

                await EmailSendLog.WriteAsync(
                    new EmailSendLogModel
                    {
                        Message = "Email successfully sent via SMTP",
                        Subject = result.EmailSubject,
                        Recipients = result.EmailAddress,
                        Server = result.Server
                    });

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

        #region Dependencies

        public GovNotifyEmailProvider GovNotifyEmailProvider { get; }

        public SmtpEmailProvider SmtpEmailProvider { get; }

        public EmailOptions EmailOptions { get; }

        #endregion
    }
}