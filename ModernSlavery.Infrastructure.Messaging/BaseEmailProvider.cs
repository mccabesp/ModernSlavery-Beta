using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.EmailTemplates;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.Infrastructure.Messaging
{
    public abstract class BaseEmailProvider
    {
        public BaseEmailProvider(
            SharedOptions sharedOptions,
            IEmailTemplateRepository emailTemplateRepo,
            ILogger logger,
            [KeyFilter(Filenames.EmailSendLog)] IAuditLogger emailSendLog)
        {
            EmailTemplateRepo = emailTemplateRepo ?? throw new ArgumentNullException(nameof(emailTemplateRepo));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            EmailSendLog = emailSendLog ?? throw new ArgumentNullException(nameof(emailSendLog));
            SharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
        }

        public virtual bool Enabled { get; } = true;
        public SharedOptions SharedOptions { get; }

        public abstract Task<SendEmailResult> SendEmailAsync<TModel>(string emailAddress, string templateId,
            TModel parameters, bool test);

        public virtual async Task<SendEmailResult> SendEmailTemplateAsync<TTemplate>(TTemplate parameters)
            where TTemplate : EmailTemplate
        {
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters), "Email template parameters are null");

            var emailTemplateType = parameters.GetType();
            var emailTemplate = EmailTemplateRepo.GetByType(emailTemplateType);
            if (emailTemplate == null)
                new NullReferenceException($"Could not find email template by type {emailTemplateType.FullName}");

            // check if this is a simulation
            if (parameters.Simulate)
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
                    parameters,
                    parameters.Test);

                return results.FirstOrDefault();
            }

            // send email using the provider implementation
            return await SendEmailAsync(parameters.RecipientEmailAddress, emailTemplate.TemplateId, parameters,
                parameters.Test);
        }

        public virtual async Task<List<SendEmailResult>> SendDistributionEmailAsync<TModel>(string emailAddresses,
            string templateId,
            TModel model,
            bool test)
        {
            var emailList = emailAddresses.SplitI(";").ToList();
            emailList = emailList.RemoveI("sender", "recipient");
            if (emailList.Count == 0) throw new ArgumentNullException(nameof(emailList));

            if (emailList.ContainsAllEmails() == false)
                throw new ArgumentException($"{emailList} contains an invalid email address", nameof(emailList));

            var successCount = 0;
            var results = new List<SendEmailResult>();
            foreach (var emailAddress in emailList)
                try
                {
                    var result = await SendEmailAsync(emailAddress, templateId, model, test);

                    await EmailSendLog.WriteAsync(
                        new EmailSendLogModel
                        {
                            Message = "Email successfully sent via SMTP",
                            Subject = result.EmailSubject,
                            Recipients = result.EmailAddress,
                            Server = result.Server,
                            Username = result.ServerUsername,
                            Details = result.EmailMessagePlainText
                        });

                    successCount++;

                    results.Add(result);
                }
                catch (Exception ex)
                {
                    Logger.LogError(
                        ex,
                        "{FuncName}: Could not send email directly to {Email}. TemplateId: '{Template}'",
                        nameof(SendDistributionEmailAsync),
                        emailAddress,
                        templateId);
                }

            return results;
        }

        #region Dependencies

        public IEmailTemplateRepository EmailTemplateRepo { get; }

        public ILogger Logger { get; }
        public IAuditLogger EmailSendLog { get; }

        #endregion
    }
}