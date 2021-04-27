namespace ModernSlavery.Core.EmailTemplates
{
    public class SendPINTemplate : EmailTemplate
    {
        public string Date { get; set; }

        public string PIN { get; set; }
        public string OrganisationName { get; set; }
        public string Url { get; set; }
        public string ExpiresDate { get; set; }
    }
}