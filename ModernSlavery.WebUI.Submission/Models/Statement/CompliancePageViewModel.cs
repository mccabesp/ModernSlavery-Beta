using ModernSlavery.WebUI.GDSDesignSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Parsers;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.WebUI.Submission.Models
{
    public class CompliancePageViewModel : GovUkViewModel, IValidatableObject
    {
        public int Year { get; set; }
        public string OrganisationIdentifier { get; set; }

        public bool? IncludesStructure { get; set; }

        public string StructureDetails { get; set; }

        public bool? IncludesPolicies { get; set; }

        public string PolicyDetails { get; set; }

        public bool? IncludesRisks { get; set; }

        public string RisksDetails { get; set; }

        public bool? IncludesDueDiligence { get; set; }

        public string DueDiligenceDetails { get; set; }

        public bool? IncludesTraining { get; set; }

        public string TrainingDetails { get; set; }

        public bool? IncludesGoals { get; set; }

        public string GoalsDetails { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();
            if (IncludesStructure == false && StructureDetails.IsNullOrWhiteSpace())
                validationResults.Add(new ValidationResult("Please provide the detail for: Your organisatio's structure, business and supply chains"));
            if (IncludesPolicies == false && PolicyDetails.IsNullOrWhiteSpace())
                validationResults.Add(new ValidationResult("Please provide the detail for: Policies"));
            if (IncludesRisks == false && RisksDetails.IsNullOrWhiteSpace())
                validationResults.Add(new ValidationResult("Please provide the detail for: Risk assessment and management"));
            if (IncludesDueDiligence == false && DueDiligenceDetails.IsNullOrWhiteSpace())
                validationResults.Add(new ValidationResult("Please provide the detail for: Due diligence processes"));
            if (IncludesTraining == false && TrainingDetails.IsNullOrWhiteSpace())
                validationResults.Add(new ValidationResult("Please provide the detail for: Staff training about slavery and human trafficking"));
            if (IncludesGoals == false && GoalsDetails.IsNullOrWhiteSpace())
                validationResults.Add(new ValidationResult("Please provide the detail for: Goals and key performance indicators (KPIs) to measure your progress over time, and the effectiveness of your actions"));
            return validationResults;

        }
    }
}
