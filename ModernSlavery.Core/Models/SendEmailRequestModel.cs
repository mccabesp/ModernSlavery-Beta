using System.Collections.Generic;

namespace ModernSlavery.Core.Models
{
    public class SendEmailRequestModel
    {

        public string EmailAddress { get; set; }
        public string TemplateId { get; set; }
        public Dictionary<string, dynamic> Personalisation { get; set; } = null;

    }
}
