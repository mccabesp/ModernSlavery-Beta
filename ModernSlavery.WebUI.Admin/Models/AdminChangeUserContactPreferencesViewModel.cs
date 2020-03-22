using ModernSlavery.WebUI.GDSDesignSystem;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;

namespace ModernSlavery.WebUI.Admin.Models
{
    public class AdminChangeUserContactPreferencesViewModel : GovUkViewModel
    {

        public long UserId { get; set; }
        public string FullName { get; set; }

        public bool AllowContact { get; set; }
        public bool SendUpdates { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        public string Reason { get; set; }

    }
}
