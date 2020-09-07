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
        private readonly SharedOptions SharedOptions;

        public SendEmailService(SharedOptions sharedOptions, EmailOptions emailOptions,
            ILogger<SendEmailService> logger, [KeyFilter(QueueNames.SendEmail)] IQueue sendEmailQueue)
        {
            SharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            EmailOptions = emailOptions ?? throw new ArgumentNullException(nameof(emailOptions));
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
        public async Task<bool> SendGeoMessageAsync(string subject, string message, bool test = false)
        {
            return await QueueEmailAsync(new QueueWrapper(new SendGeoMessageModel
            { subject = subject, message = message, test = test }));
        }

        public async Task<bool> SendCreateAccountPendingVerificationAsync(string verifyUrl, string emailAddress)
        {
            var createAccountPendingTemplate = new CreateAccountPendingVerificationTemplate
            {
                Url = verifyUrl,
                RecipientEmailAddress = emailAddress,
                Test = emailAddress.StartsWithI(SharedOptions.TestPrefix)
            };

            return await QueueEmailAsync(createAccountPendingTemplate);
        }

        public async Task<bool> SendChangeEmailPendingVerificationAsync(string verifyUrl, string emailAddress)
        {
            var changeEmailPendingTemplate = new ChangeEmailPendingVerificationTemplate
            {
                Url = verifyUrl,
                RecipientEmailAddress = emailAddress,
                Test = emailAddress.StartsWithI(SharedOptions.TestPrefix)
            };

            return await QueueEmailAsync(changeEmailPendingTemplate);
        }

        public async Task<bool> SendChangeEmailCompletedVerificationAsync(string emailAddress)
        {
            var changeEmailCompletedVerification = new ChangeEmailCompletedVerificationTemplate
            {
                RecipientEmailAddress = emailAddress,
                Test = emailAddress.StartsWithI(SharedOptions.TestPrefix)
            };

            return await QueueEmailAsync(changeEmailCompletedVerification);
        }

        public async Task<bool> SendChangeEmailCompletedNotificationAsync(string emailAddress)
        {
            var changeEmailCompletedNotification = new ChangeEmailCompletedNotificationTemplate
            {
                RecipientEmailAddress = emailAddress,
                Test = emailAddress.StartsWithI(SharedOptions.TestPrefix)
            };

            return await QueueEmailAsync(changeEmailCompletedNotification);
        }

        public async Task<bool> SendChangePasswordNotificationAsync(string emailAddress)
        {
            var changePasswordCompleted = new ChangePasswordCompletedTemplate
            {
                RecipientEmailAddress = emailAddress,
                Test = emailAddress.StartsWithI(SharedOptions.TestPrefix)
            };

            return await QueueEmailAsync(changePasswordCompleted);
        }

        public async Task<bool> SendResetPasswordNotificationAsync(string resetUrl, string emailAddress)
        {
            var resetPasswordVerification = new ResetPasswordVerificationTemplate
            {
                Url = resetUrl,
                RecipientEmailAddress = emailAddress,
                Test = emailAddress.StartsWithI(SharedOptions.TestPrefix),
                Simulate = emailAddress.StartsWithI(SharedOptions.TestPrefix)
            };

            return await QueueEmailAsync(resetPasswordVerification);
        }

        public async Task<bool> SendResetPasswordCompletedAsync(string emailAddress)
        {
            var resetPasswordCompleted = new ResetPasswordCompletedTemplate
            {
                RecipientEmailAddress = emailAddress,
                Test = emailAddress.StartsWithI(SharedOptions.TestPrefix),
                Simulate = emailAddress.StartsWithI(SharedOptions.TestPrefix)
            };

            return await QueueEmailAsync(resetPasswordCompleted);
        }

        public async Task<bool> SendAccountClosedNotificationAsync(string emailAddress, bool test)
        {
            var closeAccountCompleted = new CloseAccountCompletedTemplate
            { RecipientEmailAddress = emailAddress, Test = test };

            return await QueueEmailAsync(closeAccountCompleted);
        }

        public async Task<bool> SendGEOOrphanOrganisationNotificationAsync(string organisationName, bool test)
        {
            var orphanOrganisationTemplate = new OrphanOrganisationTemplate
            {
                RecipientEmailAddress = EmailOptions.AdminDistributionList,
                OrganisationName = organisationName,
                Test = test
            };

            return await QueueEmailAsync(orphanOrganisationTemplate);
        }

        public async Task<bool> SendGEORegistrationRequestAsync(string reviewUrl,
            string contactName,
            string reportingOrg,
            string reportingAddress,
            bool test = false)
        {
            var geoOrganisationRegistrationRequest = new MsuOrganisationRegistrationRequestTemplate
            {
                RecipientEmailAddress = EmailOptions.AdminDistributionList,
                Test = test,
                Name = contactName,
                Org2 = reportingOrg,
                Address = reportingAddress,
                Url = reviewUrl
            };

            return await QueueEmailAsync(geoOrganisationRegistrationRequest);
        }

        public async Task<bool> SendRegistrationApprovedAsync(string returnUrl, string emailAddress, bool test = false)
        {
            var organisationRegistrationApproved = new OrganisationRegistrationApprovedTemplate
            {
                RecipientEmailAddress = emailAddress,
                Test = emailAddress.StartsWithI(SharedOptions.TestPrefix),
                Simulate = emailAddress.StartsWithI(SharedOptions.TestPrefix),
                Url = returnUrl
            };

            return await QueueEmailAsync(organisationRegistrationApproved);
        }

        public async Task<bool> SendRegistrationDeclinedAsync(string emailAddress, string reason)
        {
            var organisationRegistrationDeclined = new OrganisationRegistrationDeclinedTemplate
            {
                RecipientEmailAddress = emailAddress,
                Test = emailAddress.StartsWithI(SharedOptions.TestPrefix),
                Simulate = emailAddress.StartsWithI(SharedOptions.TestPrefix),
                Reason = reason
            };

            return await QueueEmailAsync(organisationRegistrationDeclined);
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