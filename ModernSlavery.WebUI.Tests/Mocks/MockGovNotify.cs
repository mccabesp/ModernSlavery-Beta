using System.Collections.Generic;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.WebUI.Tests.Mocks
{
    public class MockGovNotify : IGovNotifyAPI
    {

        public SendEmailResponse SendEmail(SendEmailRequest sendEmailRequest)
        {
            return new SendEmailResponse { EmailId = "MOCK_RESPONSE_ID"};
        }


        public SendLetterResponse SendLetter(string templateId,
            Dictionary<string, dynamic> personalisation,
            string clientReference = null)
        {
            return new SendLetterResponse { LetterId = "MOCK_RESPONSE_ID"};
        }

    }
}
