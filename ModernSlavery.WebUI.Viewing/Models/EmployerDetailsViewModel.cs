using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.Viewing.Classes;

namespace ModernSlavery.WebUI.Viewing.Models
{
    public class EmployerDetailsViewModel
    {
        public Organisation Organisation { get; set; }

        public string LastSearchUrl { get; set; }

        public string EmployerBackUrl { get; set; }

        public SessionList<string> ComparedEmployers { get; set; }
    }
}