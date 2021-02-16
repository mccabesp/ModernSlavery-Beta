using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Admin.Models
{
    public class AdminResendVerificationEmailViewModel : GovUkViewModel
    {
        [BindNever] 
        public User User { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        [Text]
        public string Reason { get; set; }

        [BindNever]
        public object OtherErrorMessagePlaceholder { get; set; }
    }
}