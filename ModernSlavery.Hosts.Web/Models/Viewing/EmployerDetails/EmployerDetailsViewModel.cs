using ModernSlavery.WebUI.Classes;

namespace ModernSlavery.WebUI.Models.Viewing.EmployerDetails
{
    public class EmployerDetailsViewModel
    {

        public Core.Entities.Organisation Organisation { get; set; }

        public string LastSearchUrl { get; set; }

        public string EmployerBackUrl { get; set; }

        public SessionList<string> ComparedEmployers { get; set; }

    }
}
