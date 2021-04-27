using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.BusinessDomain.Shared.Classes
{
    public class NotificationService : INotificationService
    {
        private readonly SharedOptions _sharedOptions;
        private readonly EmailOptions _emailOptions;
        private readonly ILogger _logger;
        private readonly IEventLogger _customLogger;
        private readonly IQueue _sendNotifyEmailQueue;
        private readonly IGovNotifyAPI _govNotifyApi;

        public NotificationService(SharedOptions sharedOptions, 
            EmailOptions emailOptions, 
            ILogger<NotificationService> logger,
            IEventLogger customLogger, 
            [KeyFilter(QueueNames.SendNotifyEmail)]IQueue sendNotifyEmailQueue,
            IGovNotifyAPI govNotifyApi)
        {
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            _emailOptions = emailOptions ?? throw new ArgumentNullException(nameof(emailOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customLogger = customLogger ?? throw new ArgumentNullException(nameof(customLogger));
            _sendNotifyEmailQueue = sendNotifyEmailQueue ?? throw new ArgumentNullException(nameof(sendNotifyEmailQueue));
            _govNotifyApi = govNotifyApi ?? throw new ArgumentNullException(nameof(govNotifyApi));
        }

        public async Task SendRemovedUserFromOrganisationEmailAsync(string emailAddress, string organisationName,
            string removedUserName, string removedByUserName)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"OrganisationName", organisationName},
                {"RemovedUser", removedUserName},
                {"Environment", _sharedOptions.IsProduction() ? "" : $"[{_sharedOptions.Environment}] "},
                {"CurrentUser", removedByUserName }
            };

            var notifyEmail = new SendEmailRequest
            {
                EmailAddress = emailAddress,
                TemplateId = _emailOptions.Templates["OrganisationRegistrationRemovedTemplate"],
                Personalisation = personalisation
            };

            await AddEmailToQueueAsync(notifyEmail);
        }

        public async Task SendScopeChangeOutEmailAsync(string emailAddress,
            string organisationName,
            string contactName,
            string period,
            string address,
            string[] reasons)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                { "Environment", _sharedOptions.IsProduction() ? "" : $"[{_sharedOptions.Environment}] " },
                { "ORGANISATION NAME", organisationName },
                { "CONTACT NAME", contactName },
                { "PERIOD", period },
                { "REGISTERED ADDRESS", address },
            };

            for (var i = 0; i < 5; i++)
            {
                var hasreason = reasons.Length > i;
                personalisation.Add($"HASREASON{i + 1}", hasreason ? "yes" : "no");
                personalisation.Add($"REASON{i + 1}", hasreason ? reasons[i] : "");
            }

            var notifyEmail = new SendEmailRequest
            {
                EmailAddress = emailAddress,
                TemplateId = _emailOptions.Templates["ScopeChangeOutEmail"],
                Personalisation = personalisation
            };

            await AddEmailToQueueAsync(notifyEmail);
        }

        private async Task<bool> AddEmailToQueueAsync(SendEmailRequest notifyEmail)
        {
            try
            {
                await _sendNotifyEmailQueue.AddMessageAsync(notifyEmail);

                _customLogger.Information("Successfully added message to SendNotifyEmail Queue", new { notifyEmail });
                return true;
            }
            catch (Exception ex)
            {
                _customLogger.Error("Failed to add message to SendNotifyEmail Queue", new { Exception = ex });
            }

            return false;
        }
    }
}