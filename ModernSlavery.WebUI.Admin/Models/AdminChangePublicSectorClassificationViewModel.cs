using System.Collections.Generic;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.GDSDesignSystem;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;

namespace ModernSlavery.WebUI.Admin.Models
{
    public class AdminChangePublicSectorClassificationViewModel : GovUkViewModel
    {
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }

        public List<PublicSectorType> PublicSectorTypes { get; set; }

        public int? SelectedPublicSectorTypeId { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change")]
        public string Reason { get; set; }
    }
}