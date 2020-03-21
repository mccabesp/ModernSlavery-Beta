using System;
using System.Collections.Generic;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Exceptions;

namespace ModernSlavery.Infrastructure.Messaging
{
    public class GovNotifyAPI : IGovNotifyAPI
    {
        private readonly NotificationClient _client;
        private readonly IEventLogger CustomLogger;

        private readonly GovNotifyOptions Options;

        public GovNotifyAPI(GovNotifyOptions options, IEventLogger customLogger)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            CustomLogger = customLogger ?? throw new ArgumentNullException(nameof(customLogger));
            _client = new NotificationClient(Options.ApiKey);
        }

        public SendEmailResponse SendEmail(SendEmailRequest sendEmailRequest)
        {
            try
            {
                var response = _client.SendEmail(
                    sendEmailRequest.EmailAddress,
                    sendEmailRequest.TemplateId,
                    sendEmailRequest.Personalisation);

                return response == null ? null : new SendEmailResponse {EmailId = response.id};
            }
            catch (NotifyClientException e)
            {
                CustomLogger.Error(
                    "EMAIL FAILURE: Error whilst sending email using Gov.UK Notify",
                    new
                    {
                        NotifyEmail = sendEmailRequest,
                        Personalisation = JsonConvert.SerializeObject(sendEmailRequest.Personalisation),
                        ErrorMessageFromNotify = e.Message
                    });
                throw;
            }
        }

        public SendLetterResponse SendLetter(string templateId,
            Dictionary<string, dynamic> personalisation,
            string clientReference = null)
        {
            try
            {
                var response = _client.SendLetter(templateId, personalisation, clientReference);

                return response == null ? null : new SendLetterResponse {LetterId = response.id};
            }
            catch (NotifyClientException e)
            {
                CustomLogger.Error(
                    "Error whilst sending letter using Gov.UK Notify",
                    new
                    {
                        NotifyTemplateId = templateId,
                        Personalisation = JsonConvert.SerializeObject(personalisation),
                        ErrorMessageFromNotify = e.Message
                    });

                return null;
            }
        }
    }
}