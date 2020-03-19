using System;
using System.Collections.Generic;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Infrastructure.Logging;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Exceptions;
using Notify.Models.Responses;

namespace ModernSlavery.Infrastructure.Message
{
    public class GovNotifyAPI : IGovNotifyAPI
    {
        public GovNotifyAPI(GovNotifyOptions options,  ICustomLogger customLogger)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            CustomLogger = customLogger ?? throw new ArgumentNullException(nameof(customLogger));
            _client = new NotificationClient(Options.ApiKey);
        }

        private readonly GovNotifyOptions Options;
        private readonly ICustomLogger CustomLogger;
        private readonly NotificationClient _client;

        public SendEmailResponse SendEmail(SendEmailRequest sendEmailRequest)
        {
            try
            {
                EmailNotificationResponse response = _client.SendEmail(
                    sendEmailRequest.EmailAddress,
                    sendEmailRequest.TemplateId,
                    sendEmailRequest.Personalisation);

                return response==null ? null : new SendEmailResponse {EmailId = response.id};
            }
            catch (NotifyClientException e)
            {
                CustomLogger.Error(
                    "EMAIL FAILURE: Error whilst sending email using Gov.UK Notify",
                    new {
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
                LetterNotificationResponse response = _client.SendLetter(templateId, personalisation, clientReference);

                return response==null ? null : new SendLetterResponse{ LetterId= response.id};
            }
            catch (NotifyClientException e)
            {
                CustomLogger.Error(
                    "Error whilst sending letter using Gov.UK Notify",
                    new {
                        NotifyTemplateId = templateId,
                        Personalisation = JsonConvert.SerializeObject(personalisation),
                        ErrorMessageFromNotify = e.Message
                    });

                return null;
            }
        }

    }
}
