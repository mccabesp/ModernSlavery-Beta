using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Models.CompaniesHouse;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Admin.Models
{
    public class AdminChangeCompaniesHouseOptInOutViewModel : GovUkViewModel
    {
        [BindNever] 
        public Organisation Organisation { get; set; }

        [BindNever] 
        public CompaniesHouseCompany CompaniesHouseCompany { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        [Text]
        public string Reason { get; set; }
    }
}