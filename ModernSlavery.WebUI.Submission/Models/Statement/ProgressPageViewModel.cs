using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models
{
    public class ProgressPageViewModel : GovUkViewModel, IValidatableObject
    {
        public int Year { get; set; }
        public string OrganisationIdentifier { get; set; }
        [Display(Name = "Does you modern slavery statement include goals relating to how you will prevent modern slavery in your operations and supply chains?")]
        public bool IncludesMeasuringProgress { get; set; }

        [Display(Name = "How is your organisation measuring progress towards these goals?")]
        public string ProgressMeasures { get; set; }
        [Display(Name = "What were your key achievements in relation to reducing modern slavery during the period covered by this statement?")]
        public string KeyAchievements { get; set; }

        [Display(Name = "How many years has your organisation been producing modern slavery statements?")]
        public NumberOfYearsOfStatements? NumberOfYearsOfStatements { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return null;
        }

    }
    public enum NumberOfYearsOfStatements : byte
    {
        [GovUkRadioCheckboxLabelText(Text = "This is the first time")]
        thisIsTheFirstTime = 0,
        [GovUkRadioCheckboxLabelText(Text = "1 - 5 years")]
        from1To5Years = 1,
        [GovUkRadioCheckboxLabelText(Text = "More than 5 years")]
        moreThan5Years = 2,

    }
}
