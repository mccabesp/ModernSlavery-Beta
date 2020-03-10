using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.Entities.Enums;

namespace ModernSlavery.WebUI.Admin.Models
{
    public class AdminChangeScopeViewModel : GovUkViewModel
    {

        public long OrganisationId { get; set; }

        public string OrganisationName { get; set; }
        public ScopeStatuses? CurrentScopeStatus { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        public string Reason { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please select a new scope.")]
        public NewScopeStatus? NewScopeStatus { get; set; }

        public int ReportingYear { get; set; }

    }

    public enum NewScopeStatus
    {

        InScope = 0,
        OutOfScope = 1

    }

}