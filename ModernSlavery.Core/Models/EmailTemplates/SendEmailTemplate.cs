using ModernSlavery.Core.Abstractions;

namespace ModernSlavery.Core.Models
{

    public class SendEmailTemplate : AEmailTemplate
    {

        public string Subject { get; set; }

        public string MessageBody { get; set; }

    }

}
