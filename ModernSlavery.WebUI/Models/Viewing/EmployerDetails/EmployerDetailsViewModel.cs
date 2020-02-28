using ModernSlavery.Core.Classes;
using ModernSlavery.WebUI.Shared.Models;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Abstractions;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.SharedKernel;
using ModernSlavery.Entities;
using ModernSlavery.Entities.Enums;
using ModernSlavery.WebUI.Shared.Models;

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
