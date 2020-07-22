using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models
{
    public class PoliciesPageViewModel : GovUkViewModel, IValidatableObject
    {
        public int Year { get; set; }
        public string OrganisationIdentifier { get; set; }

        public IList<PolicyViewModel> Policies { get; set; }

        public string OtherPolicies { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //better way to identify this checkbox
            if (Policies.Single(x => x.Description == "Other").IsSelected && OtherPolicies.IsNullOrWhiteSpace())
                yield return new ValidationResult("Please provide detail on 'other'");
        }

        public class PolicyViewModel
        {
            public short Id { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}

