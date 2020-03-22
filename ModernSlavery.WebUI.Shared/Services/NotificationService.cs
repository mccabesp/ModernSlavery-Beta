using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.WebUI.Shared.Services
{
    public class NotificationService: INotificationService
    {
        public NotificationService(GlobalOptions globalOptions, ILogger<NotificationService> logger, IEventLogger customLogger, [KeyFilter(QueueNames.SendNotifyEmail)]IQueue sendNotifyEmailQueue)
        {
            GlobalOptions = globalOptions ?? throw new ArgumentNullException(nameof(globalOptions));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            CustomLogger = customLogger ?? throw new ArgumentNullException(nameof(customLogger));
            SendNotifyEmailQueue = sendNotifyEmailQueue ?? throw new ArgumentNullException(nameof(sendNotifyEmailQueue));
        }

        private readonly GlobalOptions GlobalOptions;
        private ILogger Logger { get; }
        private IEventLogger CustomLogger { get; }
        public IQueue SendNotifyEmailQueue { get; }

        public async void SendSuccessfulSubmissionEmail(string emailAddress,
            string organisationName,
            string submittedOrUpdated,
            string reportingPeriod,
            string reportLink)
        {
            var personalisation = new Dictionary<string, dynamic> {
                {"OrganisationName", organisationName},
                {"SubmittedOrUpdated", submittedOrUpdated},
                {"ReportingPeriod", reportingPeriod},
                {"ReportLink", reportLink},
                {"Environment", GlobalOptions.IsProduction() ? "" : $"[{GlobalOptions.Environment}] "}
            };

            var notifyEmail = new SendEmailRequest
            {
                EmailAddress = emailAddress, TemplateId = EmailTemplates.SendSuccessfulSubmissionEmail, Personalisation = personalisation
            };

            await AddEmailToQueue(notifyEmail);
        }

        public async void SendPinEmail(string emailAddress, string pin, string organisationName)
        {
            var personalisation = new Dictionary<string, dynamic> {
                {"PIN", pin},
                {"OrganisationName", organisationName},
                {"Environment", GlobalOptions.IsProduction() ? "" : $"[{GlobalOptions.Environment}] "}
            };

            var notifyEmail = new SendEmailRequest
            {
                EmailAddress = emailAddress, TemplateId = EmailTemplates.SendPinEmail, Personalisation = personalisation
            };

            await AddEmailToQueue(notifyEmail);
        }

        public async void SendUserAddedToOrganisationEmail(string emailAddress, string organisationName, string username)
        {
            var personalisation = new Dictionary<string, dynamic> {
                {"OrganisationName", organisationName},
                {"Username", username},
                {"Environment", GlobalOptions.IsProduction() ? "" : $"[{GlobalOptions.Environment}] "}
            };

            var notifyEmail = new SendEmailRequest
            {
                EmailAddress = emailAddress, TemplateId = EmailTemplates.UserAddedToOrganisationEmail, Personalisation = personalisation
            };

            await AddEmailToQueue(notifyEmail);
        }

        public async void SendRemovedUserFromOrganisationEmail(string emailAddress, string organisationName, string removedUserName)
        {
            var personalisation = new Dictionary<string, dynamic> {
                {"OrganisationName", organisationName},
                {"RemovedUser", removedUserName},
                {"Environment", GlobalOptions.IsProduction() ? "" : $"[{GlobalOptions.Environment}] "}
            };

            var notifyEmail = new SendEmailRequest
            {
                EmailAddress = emailAddress, TemplateId = EmailTemplates.RemovedUserFromOrganisationEmail, Personalisation = personalisation
            };

            await AddEmailToQueue(notifyEmail);
        }

        public async void SendScopeChangeInEmail(string emailAddress, string organisationName)
        {
            var personalisation = new Dictionary<string, dynamic> {
                {"OrganisationName", organisationName}, {"Environment", GlobalOptions.IsProduction() ? "" : $"[{GlobalOptions.Environment}] "}
            };

            var notifyEmail = new SendEmailRequest
            {
                EmailAddress = emailAddress, TemplateId = EmailTemplates.ScopeChangeInEmail, Personalisation = personalisation
            };

            await AddEmailToQueue(notifyEmail);
        }

        public async void SendScopeChangeOutEmail(string emailAddress, string organisationName)
        {
            var personalisation = new Dictionary<string, dynamic> {
                {"OrganisationName", organisationName}, {"Environment", GlobalOptions.IsProduction() ? "" : $"[{GlobalOptions.Environment}] "}
            };

            var notifyEmail = new SendEmailRequest
            {
                EmailAddress = emailAddress, TemplateId = EmailTemplates.ScopeChangeOutEmail, Personalisation = personalisation
            };

            await AddEmailToQueue(notifyEmail);
        }

        public void SendUserAddedEmailToExistingUsers(Organisation organisation, User addedUser)
        {
            IEnumerable<string> emailAddressesForOrganisation = organisation.UserOrganisations
                .Select(uo => uo.User.EmailAddress)
                .Where(ea => ea != addedUser.EmailAddress);

            foreach (string emailAddress in emailAddressesForOrganisation)
            {
                SendUserAddedToOrganisationEmail(emailAddress, organisation.OrganisationName, addedUser.Fullname);
            }
        }

        public void SendSuccessfulSubmissionEmailToRegisteredUsers(Return postedReturn, string reportLink, string submittedOrUpdated)
        {
            IEnumerable<string> emailAddressesForOrganisation = postedReturn.Organisation.UserOrganisations
                .Select(uo => uo.User.EmailAddress);

            foreach (string emailAddress in emailAddressesForOrganisation)
            {
                SendSuccessfulSubmissionEmail(
                    emailAddress,
                    postedReturn.Organisation.OrganisationName,
                    submittedOrUpdated,
                    postedReturn.GetReportingPeriod(),
                    reportLink);
            }
        }

        private async Task<bool> AddEmailToQueue(SendEmailRequest notifyEmail)
        {
            try
            {
                await SendNotifyEmailQueue.AddMessageAsync(notifyEmail);

                CustomLogger.Information("Successfully added message to SendNotifyEmail Queue", new {notifyEmail});
                return true;
            }
            catch (Exception ex)
            {
                CustomLogger.Error("Failed to add message to SendNotifyEmail Queue", new {Exception = ex});
            }

            return false;
        }

    }

    public static class EmailTemplates
    {

        public const string ScopeChangeOutEmail = "a5e14ca4-9fe7-484d-a239-fc57f0324c19";
        public const string ScopeChangeInEmail = "a54efa64-33d6-4150-9484-669ff8a6c764";
        public const string RemovedUserFromOrganisationEmail = "65ecaa57-e794-4075-9c00-f13b3cb33446";
        public const string UserAddedToOrganisationEmail = "8513d426-1881-49db-92c2-11dd1fd7a30f";
        public const string SendPinEmail = "c320cf3e-d5a1-434e-95c6-84933063be8a";
        public const string SendSuccessfulSubmissionEmail = "9f690ae4-2913-4e98-b9c9-427080f210de";

    }

}
