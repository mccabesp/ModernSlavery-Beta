using ModernSlavery.Core.Abstractions;

namespace ModernSlavery.Core.Models
{

    public class OrganisationRegistrationRemovedTemplate : AEmailTemplate
    {

        public string CurrentUser { get; set; }

        public string OrganisationName { get; set; }

    }

}
