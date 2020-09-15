using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.Viewing.Classes;

namespace ModernSlavery.WebUI.Viewing.Models
{
    public class OrganisationDetailsViewModel
    {
        public Organisation Organisation { get; set; }

        public string LastSearchUrl { get; set; }

        public string OrganisationBackUrl { get; set; }
    }
}