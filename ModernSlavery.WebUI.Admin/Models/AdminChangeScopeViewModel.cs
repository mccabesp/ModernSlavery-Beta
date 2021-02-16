using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Admin.Models
{
    public class AdminChangeScopeViewModel : GovUkViewModel
    {
        public long OrganisationId { get; set; }

        [BindNever] 
        public string OrganisationName { get; set; }
        public ScopeStatuses? CurrentScopeStatus { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        [Text]
        public string Reason { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please select a new scope.")]
        public NewScopeStatus? NewScopeStatus { get; set; }

        public int ReportingYear { get; set; }
    }

    public enum NewScopeStatus : byte
    {
        InScope = 0,
        OutOfScope = 1
    }
}