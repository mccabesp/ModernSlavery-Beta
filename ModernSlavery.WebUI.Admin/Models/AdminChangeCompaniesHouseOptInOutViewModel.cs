using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Models.CompaniesHouse;
using ModernSlavery.WebUI.GDSDesignSystem;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;

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
