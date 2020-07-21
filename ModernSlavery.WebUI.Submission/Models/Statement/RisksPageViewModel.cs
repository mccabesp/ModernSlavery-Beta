using Microsoft.Azure.Management.WebSites.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models
{
    public class RisksPageViewModel : GovUkViewModel, IValidatableObject
    {
        public int Year { get; set; }
        public string OrganisationIdentifier { get; set; }

        public List<RiskViewModel> RelevantRisks { get; set; }
        [Display(Name = " If you want to specify an area not mentioned above, please provide details")]
        public string OtherRelevantRisks;

        public List<RiskViewModel> HighRisks { get; set; }
        [Display(Name = " If you want to specify an area not mentioned above, please provide details")]
        public string OtherHighRisks;

        public List<RiskViewModel> LocationRisks { get; set; }

        public class RiskViewModel
        {
            public short Id { get; set; }
            public short? ParentId { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
            //TODO: need to set this on presenter
            public string Category { get; set; }
            //TODO: need to set this on presenter
            public List<RiskViewModel> ChildRisks { get; set; }
            //TODO: need to set this on presenter
            [MaxLength(50, ErrorMessage = "Reason can only be 50 characters or less")]
            public string Details { get; set; }

        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            var validationList = new List<ValidationResult>();
            if (HighRisks.Count < 3)
                validationList.Add(new ValidationResult("Please select 3 high risk areas"));
            foreach (var item in HighRisks)
            {
                if (item.IsSelected && item.Details == null)
                    validationList.Add(new ValidationResult($"Please explain why {item.Description} is one of your highest risks"));
            }

            return validationList;
        }
    }
}
