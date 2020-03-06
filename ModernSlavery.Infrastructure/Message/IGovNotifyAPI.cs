using System.Collections.Generic;
using Notify.Models.Responses;

namespace ModernSlavery.Infrastructure.Message
{
    public interface IGovNotifyAPI
    {

        EmailNotificationResponse SendEmail(NotifyEmail notifyEmail);

        LetterNotificationResponse SendLetter(string templateId,
            Dictionary<string, dynamic> personalisation,
            string clientReference = null);

    }
}
