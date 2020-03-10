using System.Collections.Generic;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Extensions.AspNetCore;
using ModernSlavery.Infrastructure.Logging;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Exceptions;
using Notify.Models.Responses;

namespace ModernSlavery.Infrastructure.Message
{
    public class GovNotifyAPI : IGovNotifyAPI
    {
        public GovNotifyAPI(ICustomLogger customLogger)
        {
            CustomLogger = customLogger;
        }

        private readonly ICustomLogger CustomLogger;
        private readonly NotificationClient _client = new NotificationClient(_apiKey);
        private static string _apiKey => Config.GetAppSetting("Email:Providers:GovNotify:ApiKey");

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
