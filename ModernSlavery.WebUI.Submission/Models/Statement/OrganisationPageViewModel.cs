using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models
{
    public class OrganisationPageViewModel : GovUkViewModel, IValidatableObject
    {
        public int Year { get; set; }
        public string OrganisationIdentifier { get; set; }

        public IList<SectorViewModel> Sectors { get; set; }

        [Display(Name = "What was your turnover or budget during the last financial accounting year?")]
        public LastFinancialYearBudget? Turnover { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return null;
        }

        public class SectorViewModel
        {
            public short Id { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }

        }

    }

    public enum LastFinancialYearBudget : byte
    {
        [GovUkRadioCheckboxLabelText(Text = "Under £36 million")]
        Under36Million = 0,

        [GovUkRadioCheckboxLabelText(Text = "£36 million - £60 million")]
        From36MillionTo60Million = 1,

        [GovUkRadioCheckboxLabelText(Text = "£60 million - £100 million")]
        From60MillionTo100Million = 2,

        [GovUkRadioCheckboxLabelText(Text = "£100 million - £500 million")]
        From100MillionTo500Million = 3,

        [GovUkRadioCheckboxLabelText(Text = "£500 million+")]
        From500MillionUpwards = 4,

    }

}
