using System.Collections.Generic;
using ModernSlavery.Core.Classes;
using Notify.Models.Responses;

namespace ModernSlavery.Core.Interfaces
{
    public interface IGovNotifyAPI
    {

        EmailNotificationResponse SendEmail(NotifyEmail notifyEmail);

        LetterNotificationResponse SendLetter(string templateId,
            Dictionary<string, dynamic> personalisation,
            string clientReference = null);

    }
}
