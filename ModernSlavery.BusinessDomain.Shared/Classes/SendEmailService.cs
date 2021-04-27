using System;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.EmailTemplates;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.BusinessDomain.Shared.Classes
{
    public class SendEmailService : ISendEmailService
    {
        private readonly EmailOptions EmailOptions;
        private readonly TestOptions TestOptions;

        public SendEmailService(EmailOptions emailOptions, TestOptions testOptions,
            ILogger<SendEmailService> logger, [KeyFilter(QueueNames.SendEmail)] IQueue sendEmailQueue)
        {
            EmailOptions = emailOptions ?? throw new ArgumentNullException(nameof(emailOptions));
            TestOptions = testOptions ?? throw new ArgumentNullException(nameof(testOptions));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            SendEmailQueue = sendEmailQueue ?? throw new ArgumentNullException(nameof(sendEmailQueue));
        }

        private ILogger Logger { get; }
        private IQueue SendEmailQueue { get; }

        /// <summary>
        ///     Send a message to GEO distribution list
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        public async Task<bool> SendMsuMessageAsync(string subject, string message)
        {
            return await QueueEmailAsync(new SendMsuMessageModel{ subject = subject, message = message });
        }

        public async Task<bool> SendCreateAccountPendingVerificationAsync(string verifyUrl, string emailAddress)
        {
            var createAccountPendingTemplate = new CreateAccountPendingVerificationTemplate
            {
                Url = verifyUrl,
                RecipientEmailAddress = emailAddress
            };

            return await QueueEmailAsync(createAccountPendingTemplate);
        }

        public async Task<bool> SendChangeEmailPendingVerificationAsync(string verifyUrl, string emailAddress)
        {
            var changeEmailPendingTemplate = new ChangeEmailPendingVerificationTemplate
            {
                Url = verifyUrl,
                RecipientEmailAddress = emailAddress
            };

            return await QueueEmailAsync(changeEmailPendingTemplate);
        }

        public async Task<bool> SendChangeEmailCompletedVerificationAsync(string emailAddress)
        {
            var changeEmailCompletedVerification = new ChangeEmailCompletedVerificationTemplate
            {
                RecipientEmailAddress = emailAddress
            };

            return await QueueEmailAsync(changeEmailCompletedVerification);
        }

        public async Task<bool> SendChangeEmailCompletedNotificationAsync(string emailAddress)
        {
            var changeEmailCompletedNotification = new ChangeEmailCompletedNotificationTemplate
            {
                RecipientEmailAddress = emailAddress
            };

            return await QueueEmailAsync(changeEmailCompletedNotification);
        }

        public async Task<bool> SendChangePasswordNotificationAsync(string emailAddress)
        {
            var changePasswordCompleted = new ChangePasswordCompletedTemplate
            {
                RecipientEmailAddress = emailAddress
            };

            return await QueueEmailAsync(changePasswordCompleted);
        }

        public async Task<bool> SendResetPasswordNotificationAsync(string resetUrl, string emailAddress)
        {
            var resetPasswordVerification = new ResetPasswordVerificationTemplate
            {
                Url = resetUrl,
                RecipientEmailAddress = emailAddress
            };

            return await QueueEmailAsync(resetPasswordVerification);
        }

        public async Task<bool> SendResetPasswordCompletedAsync(string emailAddress)
        {
            var resetPasswordCompleted = new ResetPasswordCompletedTemplate
            {
                RecipientEmailAddress = emailAddress
            };

            return await QueueEmailAsync(resetPasswordCompleted);
        }

        public async Task<bool> SendAccountClosedNotificationAsync(string emailAddress)
        {
            var closeAccountCompleted = new CloseAccountCompletedTemplate
            { 
                RecipientEmailAddress = emailAddress 
            };

            return await QueueEmailAsync(closeAccountCompleted);
        }

        public async Task<bool> SendMsuOrphanOrganisationNotificationAsync(string organisationName)
        {
            var orphanOrganisationTemplate = new OrphanOrganisationTemplate
            {
                RecipientEmailAddress = EmailOptions.AdminDistributionList,
                OrganisationName = organisationName
            };

            return await QueueEmailAsync(orphanOrganisationTemplate);
        }

        public async Task<bool> SendMsuRegistrationRequestAsync(string reviewUrl,
            string contactName,
            string reportingOrg,
            string reportingAddress,
            string details=null)
        {
            var geoOrganisationRegistrationRequest = new MsuOrganisationRegistrationRequestTemplate {
                RecipientEmailAddress = EmailOptions.AdminDistributionList,
                Name = contactName,
                Org2 = reportingOrg,
                Address = reportingAddress,
                Url = reviewUrl,
                Details = string.IsNullOrWhiteSpace(details) ? " " : details
            };

            return await QueueEmailAsync(geoOrganisationRegistrationRequest);
        }

        public async Task<bool> SendRegistrationApprovedAsync(string organisationName, string returnUrl, string emailAddress)
        {
            var organisationRegistrationApproved = new OrganisationRegistrationApprovedTemplate
            {
                RecipientEmailAddress = emailAddress,
                OrganisationName = organisationName,
                Url = returnUrl
            };

            return await QueueEmailAsync(organisationRegistrationApproved);
        }

        public async Task<bool> SendRegistrationDeclinedAsync(string organisationName, string emailAddress, string reason)
        {
            var organisationRegistrationDeclined = new OrganisationRegistrationDeclinedTemplate
            {
                RecipientEmailAddress = emailAddress,
                OrganisationName = organisationName,
                Reason = reason
            };

            return await QueueEmailAsync(organisationRegistrationDeclined);
        }

        public async Task<bool> SendPinEmailAsync(string emailAddress, string pin, string organisationName, string url, DateTime expiresDate)
        {
            var sendPINTemplate = new SendPINTemplate {
                RecipientEmailAddress=emailAddress,
                PIN = pin,
                OrganisationName = organisationName,
                Date = VirtualDateTime.Now.ToString("d MMMM yyyy"),
                Url = url,
                ExpiresDate = expiresDate.ToString("d MMMM yyyy"),
            };

            return await QueueEmailAsync(sendPINTemplate);
        }

        private async Task<bool> QueueEmailAsync<TTemplate>(TTemplate emailTemplate)
        {
            try
            {
                await SendEmailQueue.AddMessageAsync(new QueueWrapper(emailTemplate));
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
            }

            return false;
        }
    }
}