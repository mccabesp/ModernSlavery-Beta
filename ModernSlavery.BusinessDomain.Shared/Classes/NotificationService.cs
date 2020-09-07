using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.BusinessDomain.Shared.Classes
{
    public class NotificationService : INotificationService
    {
        private readonly SharedOptions _sharedOptions;
        private readonly EmailOptions _emailOptions;

        public NotificationService(SharedOptions sharedOptions, EmailOptions emailOptions, ILogger<NotificationService> logger,
            IEventLogger customLogger, [KeyFilter(QueueNames.SendNotifyEmail)]
            IQueue sendNotifyEmailQueue)
        {
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            _emailOptions = emailOptions ?? throw new ArgumentNullException(nameof(emailOptions));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            CustomLogger = customLogger ?? throw new ArgumentNullException(nameof(customLogger));
            SendNotifyEmailQueue =
                sendNotifyEmailQueue ?? throw new ArgumentNullException(nameof(sendNotifyEmailQueue));
        }

        private ILogger Logger { get; }
        private IEventLogger CustomLogger { get; }
        public IQueue SendNotifyEmailQueue { get; }

        public async void SendPinEmail(string emailAddress, string pin, string organisationName)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"PIN", pin},
                {"OrganisationName", organisationName},
                {"Environment", _sharedOptions.IsProduction() ? "" : $"[{_sharedOptions.Environment}] "}
            };

            var notifyEmail = new SendEmailRequest
            {
                EmailAddress = emailAddress,
                TemplateId = _emailOptions.Templates["SendPinEmail"],
                Personalisation = personalisation
            };

            await AddEmailToQueue(notifyEmail);
        }

        public async void SendRemovedUserFromOrganisationEmail(string emailAddress, string organisationName,
            string removedUserName)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"OrganisationName", organisationName},
                {"RemovedUser", removedUserName},
                {"Environment", _sharedOptions.IsProduction() ? "" : $"[{_sharedOptions.Environment}] "}
            };

            var notifyEmail = new SendEmailRequest
            {
                EmailAddress = emailAddress,
                TemplateId = _emailOptions.Templates["OrganisationRegistrationRemovedTemplate"],
                Personalisation = personalisation
            };

            await AddEmailToQueue(notifyEmail);
        }

        public async void SendScopeChangeOutEmail(string emailAddress,
            string organisationName,
            string contactName,
            string period,
            string address,
            string reason)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                { "Environment", _sharedOptions.IsProduction() ? "" : $"[{_sharedOptions.Environment}] " },
                { "ORGANISATION NAME", organisationName },
                { "CONTACT NAME", contactName },
                { "PERIOD", period },
                { "REGISTERED ADDRESS", address },
                { "REASON GIVEN", reason },
            };

            var notifyEmail = new SendEmailRequest
            {
                EmailAddress = emailAddress,
                TemplateId = _emailOptions.Templates["ScopeChangeOutEmail"],
                Personalisation = personalisation
            };

            await AddEmailToQueue(notifyEmail);
        }

        private async Task<bool> AddEmailToQueue(SendEmailRequest notifyEmail)
        {
            try
            {
                await SendNotifyEmailQueue.AddMessageAsync(notifyEmail);

                CustomLogger.Information("Successfully added message to SendNotifyEmail Queue", new { notifyEmail });
                return true;
            }
            catch (Exception ex)
            {
                CustomLogger.Error("Failed to add message to SendNotifyEmail Queue", new { Exception = ex });
            }

            return false;
        }
    }
}