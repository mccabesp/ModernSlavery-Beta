using System.Collections.Generic;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Core.Interfaces
{
    public interface IGovNotifyAPI
    {

        SendEmailResponse SendEmail(SendEmailRequest sendEmailRequest);

        SendLetterResponse SendLetter(string templateId,
            Dictionary<string, dynamic> personalisation,
            string clientReference = null);

    }
}
