using System;
using System.Collections.Generic;
using System.Linq;
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

namespace ModernSlavery.Infrastructure.Messaging
{
    public abstract class BaseEmailProvider
    {
        #region Dependencies
        protected readonly IEmailTemplateRepository EmailTemplateRepo;
        protected readonly ILogger Logger;
        protected readonly TestOptions _testOptions;
        public readonly IAuditLogger EmailSendLog;
        #endregion

        public BaseEmailProvider(
            TestOptions testOptions,
            IEmailTemplateRepository emailTemplateRepo,
            ILogger logger,
            [KeyFilter(Filenames.EmailSendLog)] IAuditLogger emailSendLog)
        {
            _testOptions = testOptions ?? throw new ArgumentNullException(nameof(testOptions)); 
            EmailTemplateRepo = emailTemplateRepo ?? throw new ArgumentNullException(nameof(emailTemplateRepo));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            EmailSendLog = emailSendLog ?? throw new ArgumentNullException(nameof(emailSendLog));
        }

        public virtual bool Enabled { get; } = true;

        public abstract Task<SendEmailResult> SendEmailAsync<TModel>(string emailAddress, string templateId,TModel parameters);

        public virtual async Task<SendEmailResult> SendEmailTemplateAsync<TTemplate>(TTemplate parameters)where TTemplate : EmailTemplate
        {
            if (parameters is null)throw new ArgumentNullException(nameof(parameters), "Email template parameters are null");

            var emailTemplateType = parameters.GetType();
            var emailTemplate = EmailTemplateRepo.GetByType(emailTemplateType);
            if (emailTemplate == null)throw new NullReferenceException($"Could not find email template by type {emailTemplateType.FullName}");

            // check if this is a simulation
            if (_testOptions.SimulateMessageSend)
                return new SendEmailResult
                {
                    Status = "sent",
                    Server = "Simulation",
                    ServerUsername = "TestUse",
                    EmailAddress = parameters.RecipientEmailAddress,
                    EmailSubject = emailTemplate.EmailSubject
                };

            // check if this is a distributed email
            var isDistributedEmailList = parameters.RecipientEmailAddress.Contains(";");
            if (isDistributedEmailList)
            {
                var results = await SendDistributionEmailAsync(
                    parameters.RecipientEmailAddress,
                    emailTemplate.TemplateId,
                    parameters).ConfigureAwait(false);

                return results.FirstOrDefault();
            }

            // send email using the provider implementation
            var result = await SendEmailAsync(parameters.RecipientEmailAddress, emailTemplate.TemplateId, parameters).ConfigureAwait(false);

            await EmailSendLog.WriteAsync(new EmailSendLogModel {
                Message = "Email sent via Gov Notify",
                Subject = result.EmailSubject,
                Recipients = result.EmailAddress,
                Server = result.Server,
                Username = result.ServerUsername,
                Details = result.EmailMessagePlainText
            }).ConfigureAwait(false);

            return result;
        }

        public virtual async Task<List<SendEmailResult>> SendDistributionEmailAsync<TModel>(string emailAddresses,string templateId,TModel model)
        {
            var emailList = emailAddresses.SplitI(';').ToList();
            emailList = emailList.RemoveI("sender", "recipient");
            if (emailList.Count == 0) throw new ArgumentNullException(nameof(emailList));

            if (!emailList.ContainsAllEmails())throw new ArgumentException($"{emailList} contains an invalid email address", nameof(emailList));

            var successCount = 0;
            var results = new List<SendEmailResult>();
            foreach (var emailAddress in emailList)
                try
                {
                    var result = await SendEmailAsync(emailAddress, templateId, model).ConfigureAwait(false);

                    if (result == null) continue;

                    await EmailSendLog.WriteAsync(
                        new EmailSendLogModel
                        {
                            Message = "Email successfully sent via SMTP",
                            Subject = result.EmailSubject,
                            Recipients = result.EmailAddress,
                            Server = result.Server,
                            Username = result.ServerUsername,
                            Details = result.EmailMessagePlainText
                        }).ConfigureAwait(false);

                    successCount++;

                    results.Add(result);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex,$"{nameof(SendDistributionEmailAsync)}: Could not send email directly to {emailAddress}. TemplateId: '{templateId}'");
                }

            return results;
        }
    }
}