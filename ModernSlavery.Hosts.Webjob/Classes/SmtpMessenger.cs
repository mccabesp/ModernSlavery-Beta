using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.EmailTemplates;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.Core.Options;
using ModernSlavery.Infrastructure.Messaging;

namespace ModernSlavery.Hosts.Webjob.Classes
{
    public interface ISmtpMessenger
    {
        Task<bool> SendMsuMessageAsync(string subject, string message);

        Task<bool> SendMessageAsync(string subject, string recipients, string message);

        Task SendEmailTemplateAsync<TTemplate>(TTemplate parameters) where TTemplate : EmailTemplate;
    }

    public class SmtpMessenger : ISmtpMessenger
    {
        private readonly EmailProvider _msuEmailProvider;

        private readonly ILogger<SmtpMessenger> _log;
        private readonly SmtpEmailOptions _smtpOptions; 
        private readonly TestOptions _testOptions;

        public SmtpMessenger(ILogger<SmtpMessenger> logger, SmtpEmailOptions smtpOptions, TestOptions testOptions, EmailProvider msuEmailProvider)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
            _smtpOptions = smtpOptions ?? throw new ArgumentNullException(nameof(smtpOptions));
            _testOptions = testOptions ?? throw new ArgumentNullException(nameof(testOptions));
            _msuEmailProvider = msuEmailProvider ?? throw new ArgumentNullException(nameof(msuEmailProvider));
        }

        #region Emails

        /// <summary>
        ///     Send a message to MSU distribution list
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        public async Task<bool> SendMsuMessageAsync(string subject, string message)
        {
            var emailAddresses = _msuEmailProvider.EmailOptions.AdminDistributionList.SplitI(';').ToList();
            emailAddresses = emailAddresses.RemoveI("sender", "recipient");
            if (emailAddresses.Count == 0)
                throw new ArgumentNullException(nameof(_msuEmailProvider.EmailOptions.AdminDistributionList));

            if (!emailAddresses.ContainsAllEmails())
                throw new ArgumentException($"{_msuEmailProvider.EmailOptions.AdminDistributionList} contains an invalid email address",nameof(_msuEmailProvider.EmailOptions.AdminDistributionList));

            var successCount = 0;

            foreach (var emailAddress in emailAddresses)
                try
                {
                    await Email.QuickSendAsync(
                        _testOptions.IsProduction() ? subject : $"[{_testOptions.Environment}] {subject}",
                        _smtpOptions.SenderEmail,
                        _smtpOptions.SenderName,
                        _smtpOptions.ReplyEmail,
                        emailAddress,
                        message,
                        _smtpOptions.Server,
                        _smtpOptions.Username,
                        _smtpOptions.Password,
                        _smtpOptions.Port,
                        simulate: _testOptions.SimulateMessageSend).ConfigureAwait(false);

                    await _msuEmailProvider.EmailSendLog.WriteAsync(
                        new EmailSendLogModel
                        {
                            Message = $"Email successfully sent {(_testOptions.SimulateMessageSend ? "{Simulated) " : "")}via SMTP",
                            Subject = subject,
                            Recipients = emailAddress,
                            Server = $"{_smtpOptions.Server}:{_smtpOptions.Port}",
                            Username = _smtpOptions.Username,
                            Details = message
                        }).ConfigureAwait(false);
                    successCount++;
                }
                catch (Exception ex1)
                {
                    _log.LogError(ex1, $"Cant send {(_testOptions.SimulateMessageSend ? "{Simulated) " : "")}message '{subject}' '{message}' directly to {emailAddress}:");
                }

            return successCount == emailAddresses.Count;
        }

        public async Task<bool> SendMessageAsync(string subject, string recipients, string message)
        {
            var emailAddresses = recipients.SplitI(';').ToList();
            emailAddresses = emailAddresses.RemoveI("sender", "recipient");
            if (emailAddresses.Count == 0) throw new ArgumentNullException(nameof(recipients));

            if (!emailAddresses.ContainsAllEmails())
                throw new ArgumentException($"{recipients} contains an invalid email address", nameof(recipients));

            var successCount = 0;
            foreach (var emailAddress in emailAddresses)
                try
                {
                    await Email.QuickSendAsync(
                        _testOptions.IsProduction() ? subject : $"[{_testOptions.Environment}] {subject}",
                        _smtpOptions.SenderEmail,
                        _smtpOptions.SenderName,
                        _smtpOptions.ReplyEmail,
                        emailAddress,
                        message,
                        _smtpOptions.Server,
                        _smtpOptions.Username,
                        _smtpOptions.Password,
                        _smtpOptions.Port,
                        simulate: _testOptions.SimulateMessageSend).ConfigureAwait(false);

                    await _msuEmailProvider.EmailSendLog.WriteAsync(
                        new EmailSendLogModel
                        {
                            Message = $"Email successfully sent {(_testOptions.SimulateMessageSend ? "(Simulated) " : "")}via SMTP",
                            Subject = subject,
                            Recipients = emailAddress,
                            Server = $"{_smtpOptions.Server}:{_smtpOptions.Port}",
                            Username = _smtpOptions.Username,
                            Details = message
                        }).ConfigureAwait(false);
                    successCount++;
                }
                catch (Exception ex1)
                {
                    _log.LogError(ex1, $"Cant send {(_testOptions.SimulateMessageSend ? "{Simulated) " : "")}message '{subject}' '{message}' directly to {emailAddress}:");
                }

            return successCount == emailAddresses.Count;
        }

        public async Task SendEmailTemplateAsync<TTemplate>(TTemplate parameters) where TTemplate : EmailTemplate
        {
            await _msuEmailProvider.SendEmailTemplateAsync(parameters).ConfigureAwait(false);
        }

        #endregion
    }
}