using System.Collections.Generic;

namespace ModernSlavery.Core.Models
{
    public class SendEmailRequest
    {
        public string EmailAddress { get; set; }
        public string TemplateId { get; set; }
        public Dictionary<string, dynamic> Personalisation { get; set; } = null;
    }
}