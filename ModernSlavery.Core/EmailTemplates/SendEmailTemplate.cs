namespace ModernSlavery.Core.EmailTemplates
{
    public class SendEmailTemplate : EmailTemplate
    {
        public string Subject { get; set; }

        public string MessageBody { get; set; }
    }
}