using ModernSlavery.WebUI.Viewing.Classes;

namespace ModernSlavery.WebUI.Viewing.Models.EmployerDetails
{
    public class EmployerDetailsViewModel
    {

        public Core.Entities.Organisation Organisation { get; set; }

        public string LastSearchUrl { get; set; }

        public string EmployerBackUrl { get; set; }

        public SessionList<string> ComparedEmployers { get; set; }

    }
}
