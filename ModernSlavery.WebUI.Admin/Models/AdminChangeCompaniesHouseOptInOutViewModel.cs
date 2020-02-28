using ModernSlavery.Core.Models.CompaniesHouse;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.Entities;

namespace ModernSlavery.WebUI.Admin.Models
{
    public class AdminChangeCompaniesHouseOptInOutViewModel : GovUkViewModel
    {

        public Organisation Organisation { get; set; }

        public CompaniesHouseCompany CompaniesHouseCompany { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        public string Reason { get; set; }

    }
}
