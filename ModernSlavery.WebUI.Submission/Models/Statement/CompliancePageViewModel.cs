using ModernSlavery.WebUI.GDSDesignSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Parsers;

namespace ModernSlavery.WebUI.Submission.Models
{
    public class CompliancePageViewModel : GovUkViewModel, IValidatableObject
    {
        public int Year { get; set; }
        public string OrganisationIdentifier { get; set; }

        public bool? IncludesStructure { get; set; }
        [GovUkValidateRequiredIf("IncludesStructure", "False", "Please provide detail")]
        public string StructureDetails { get; set; }

        public bool? IncludesPolicies { get; set; }
        [GovUkValidateRequiredIf("IncludesPolicies", "False", "Please provide detail")]
        public string PolicyDetails { get; set; }

        public bool? IncludesRisks { get; set; }
        [GovUkValidateRequiredIf("IncludesRisks", "False", "Please provide detail")]
        public string RisksDetails { get; set; }

        public bool? IncludesDueDiligence { get; set; }
        [GovUkValidateRequiredIf("IncludesDueDiligence", "False", "Please provide detail")]
        public string DueDiligenceDetails { get; set; }

        public bool? IncludesTraining { get; set; }
        [GovUkValidateRequiredIf("IncludesTraining", "False", "Please provide detail")]
        public string TrainingDetails { get; set; }

        public bool? IncludesGoals { get; set; }
        [GovUkValidateRequiredIf("IncludesGoals", "False", "Please provide detail")]
        public string GoalsDetails { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //Validation is done instead with the parse helper when viewModel.ParseAndValidateParameters(Request, m => m.Property) is used for all properties in the controller;

            //var validationResults = new List<ValidationResult>();
            //if (IncludesStructure == false && StructureDetails == null)
            //    validationResults.Add(new ValidationResult("Please provide the detail for: Your organisatio's structure, business and supply chains"));
            //if (IncludesPolicies == false && PolicyDetails == null)
            //    validationResults.Add(new ValidationResult("Please provide the detail for: Policies"));
            //if (IncludesRisks == false && RisksDetails == null)
            //    validationResults.Add(new ValidationResult("Please provide the detail for: Risk assessment and management"));
            //if (IncludesDueDiligence == false && DueDiligenceDetails == null)
            //    validationResults.Add(new ValidationResult("Please provide the detail for: Due diligence processes"));
            //if (IncludesTraining == false && TrainingDetails == null)
            //    validationResults.Add(new ValidationResult("Please provide the detail for: Staff training about slavery and human trafficking"));
            //if (IncludesGoals == false && GoalsDetails == null)
            //    validationResults.Add(new ValidationResult("Please provide the detail for: Goals and key performance indicators (KPIs) to measure your progress over time, and the effectiveness of your actions"));
            //return validationResults;
            return null;
        }
    }
}
