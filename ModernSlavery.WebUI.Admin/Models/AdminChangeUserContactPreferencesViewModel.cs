using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Admin.Models
{
    public class AdminChangeUserContactPreferencesViewModel : GovUkViewModel
    {
        public long UserId { get; set; }
        [BindNever] 
        public string FullName { get; set; }

        public bool AllowContact { get; set; }
        public bool SendUpdates { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        [Text]
        public string Reason { get; set; }
    }
}