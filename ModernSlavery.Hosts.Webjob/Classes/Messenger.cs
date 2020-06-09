using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.EmailTemplates;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.Infrastructure.Messaging;

namespace ModernSlavery.Hosts.Webjob.Classes
{
    public interface IMessenger
    {
        Task<bool> SendGeoMessageAsync(string subject, string message, bool test = false);

        Task<bool> SendMessageAsync(string subject, string recipients, string message, bool test = false);

        Task SendEmailTemplateAsync<TTemplate>(TTemplate parameters) where TTemplate : EmailTemplate;
    }

    public class Messenger : IMessenger
    {
        private readonly EmailProvider GpgEmailProvider;

        private readonly ILogger<Messenger> log;
        private readonly SmtpEmailOptions SmtpOptions;

        public Messenger(ILogger<Messenger> logger, SmtpEmailOptions smtpOptions, EmailProvider gpgEmailProvider)
        {
            log = logger ?? throw new ArgumentNullException(nameof(logger));
            SmtpOptions = smtpOptions ?? throw new ArgumentNullException(nameof(smtpOptions));
            GpgEmailProvider = gpgEmailProvider ?? throw new ArgumentNullException(nameof(gpgEmailProvider));
        }

        #region Emails

        /// <summary>
        ///     Send a message to GEO distribution list
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        public async Task<bool> SendGeoMessageAsync(string subject, string message, bool test = false)
        {
            var emailAddresses = GpgEmailProvider.EmailOptions.AdminDistributionList.SplitI(";").ToList();
            emailAddresses = emailAddresses.RemoveI("sender", "recipient");
            if (emailAddresses.Count == 0)
                throw new ArgumentNullException(nameof(GpgEmailProvider.EmailOptions.AdminDistributionList));

            if (!emailAddresses.ContainsAllEmails())
                throw new ArgumentException(
                    $"{GpgEmailProvider.EmailOptions.AdminDistributionList} contains an invalid email address",
                    nameof(GpgEmailProvider.EmailOptions.AdminDistributionList));

            var successCount = 0;
            foreach (var emailAddress in emailAddresses)
                try
                {
                    await Email.QuickSendAsync(
                        subject,
                        SmtpOptions.SenderEmail,
                        SmtpOptions.SenderName,
                        SmtpOptions.ReplyEmail,
                        emailAddress,
                        message,
                        SmtpOptions.Server2,
                        SmtpOptions.Username2,
                        SmtpOptions.Password2,
                        SmtpOptions.Port2,
                        test: test);
                    await GpgEmailProvider.EmailSendLog.WriteAsync(
                        new EmailSendLogModel
                        {
                            Message = "Email successfully sent via SMTP",
                            Subject = subject,
                            Recipients = emailAddress,
                            Server = $"{SmtpOptions.Server2}:{SmtpOptions.Port2}",
                            Username = SmtpOptions.Username2,
                            Details = message
                        });
                    successCount++;
                }
                catch (Exception ex1)
                {
                    log.LogError(ex1, $"Cant send message '{subject}' '{message}' directly to {emailAddress}:");
                }

            return successCount == emailAddresses.Count;
        }

        public async Task<bool> SendMessageAsync(string subject, string recipients, string message, bool test = false)
        {
            var emailAddresses = recipients.SplitI(";").ToList();
            emailAddresses = emailAddresses.RemoveI("sender", "recipient");
            if (emailAddresses.Count == 0) throw new ArgumentNullException(nameof(recipients));

            if (!emailAddresses.ContainsAllEmails())
                throw new ArgumentException($"{recipients} contains an invalid email address", nameof(recipients));

            var successCount = 0;
            foreach (var emailAddress in emailAddresses)
                try
                {
                    await Email.QuickSendAsync(
                        subject,
                        SmtpOptions.SenderEmail,
                        SmtpOptions.SenderName,
                        SmtpOptions.ReplyEmail,
                        emailAddress,
                        message,
                        SmtpOptions.Server2,
                        SmtpOptions.Username2,
                        SmtpOptions.Password2,
                        SmtpOptions.Port2,
                        test: test);
                    await GpgEmailProvider.EmailSendLog.WriteAsync(
                        new EmailSendLogModel
                        {
                            Message = "Email successfully sent via SMTP",
                            Subject = subject,
                            Recipients = emailAddress,
                            Server = $"{SmtpOptions.Server2}:{SmtpOptions.Port2}",
                            Username = SmtpOptions.Username2,
                            Details = message
                        });
                    successCount++;
                }
                catch (Exception ex1)
                {
                    log.LogError(ex1, $"Cant send message '{subject}' '{message}' directly to {emailAddress}:");
                }

            return successCount == emailAddresses.Count;
        }

        public async Task SendEmailTemplateAsync<TTemplate>(TTemplate parameters) where TTemplate : EmailTemplate
        {
            await GpgEmailProvider.SendEmailTemplateAsync(parameters);
        }

        #endregion
    }
}