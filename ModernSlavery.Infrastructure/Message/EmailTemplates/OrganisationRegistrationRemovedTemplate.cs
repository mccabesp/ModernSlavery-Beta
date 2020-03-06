namespace ModernSlavery.Infrastructure.Message.EmailTemplates
{

    public class OrganisationRegistrationRemovedTemplate : AEmailTemplate
    {

        public string CurrentUser { get; set; }

        public string OrganisationName { get; set; }

    }

}
