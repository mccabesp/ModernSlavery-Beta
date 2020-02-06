using ModernSlavery.Database;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace ModernSlavery.WebUI.Models.Admin
{
    public class AdminResendVerificationEmailViewModel : GovUkViewModel
    {
        public User User { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        public string Reason { get; set; }

        public object OtherErrorMessagePlaceholder { get; set; }
    }
}
