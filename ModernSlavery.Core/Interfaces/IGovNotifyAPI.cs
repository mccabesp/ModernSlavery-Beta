using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Core.Interfaces
{
    public interface IGovNotifyAPI
    {
        Task<SendEmailResponse> SendEmailAsync(SendEmailRequest sendEmailRequest);

        Task<SendLetterResponse> SendLetterAsync(string templateId,
            Dictionary<string, dynamic> personalisation,
            string clientReference = null);

        Task<SendEmailResult> GetEmailResultAsync(string emailId);

        
    }
}