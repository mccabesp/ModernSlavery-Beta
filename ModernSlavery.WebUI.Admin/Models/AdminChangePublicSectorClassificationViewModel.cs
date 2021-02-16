using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Admin.Models
{
    public class AdminChangePublicSectorClassificationViewModel : GovUkViewModel
    {
        public long OrganisationId { get; set; }
        [BindNever] 
        public string OrganisationName { get; set; }

        public List<PublicSectorType> PublicSectorTypes { get; set; }

        public int? SelectedPublicSectorTypeId { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change")]
        [Text]
        public string Reason { get; set; }
    }
}