using ModernSlavery.Core.Classes;

namespace ModernSlavery.WebUI.Models
{
    public class EmployerDetailsViewModel
    {

        public Entities.Organisation Organisation { get; set; }

        public string LastSearchUrl { get; set; }

        public string EmployerBackUrl { get; set; }

        public SessionList<string> ComparedEmployers { get; set; }

    }
}
