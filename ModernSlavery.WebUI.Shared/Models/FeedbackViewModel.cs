using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.GDSDesignSystem;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;

namespace ModernSlavery.WebUI.Shared.Models
{
    public class FeedbackViewModel : GovUkViewModel
    {
        [GovUkValidateRequired(
            ErrorMessageIfMissing = "Select what you did on this service"
        )]
        public WhyVisitSite? WhyVisitMSUSite { get; set; }

        [GovUkValidateRequired(
           ErrorMessageIfMissing = "Select how easy or difficult this service is to use"
         )]
        public HowEasyIsThisServiceToUse? HowEasyIsThisServiceToUse { get; set; }

        [GovUkValidateRequired(
           ErrorMessageIfMissing = "Tell us how we can improve the service"
        )]
        [GovUkValidateCharacterCount(
            MaxCharacters = 2000
        )]
        [Required]
        [MaxLength(2000)]
        public string Details { get; set; }

        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }
    }
    public enum WhyVisitSite : byte
    {
        [GovUkRadioCheckboxLabelText(Text = "Submitted a statement")]
        SubmittedAStatement = 0,

        [GovUkRadioCheckboxLabelText(Text = "Viewed 1 or more statements")]
        Viewed1OrMoreStatements = 1,

        [GovUkRadioCheckboxLabelText(Text = "Submitted and viewed statements")]
        SubmittedAndViewedStatements = 2,


    }

    public enum HowEasyIsThisServiceToUse : byte
    {
        [GovUkRadioCheckboxLabelText(Text = "Very easy")]
        VeryEasy = 0,

        [GovUkRadioCheckboxLabelText(Text = "Easy")]
        Easy = 1,

        [GovUkRadioCheckboxLabelText(Text = "Neither easy nor difficult")]
        Neutral = 2,

        [GovUkRadioCheckboxLabelText(Text = "Difficult")]
        Difficult = 3,

        [GovUkRadioCheckboxLabelText(Text = "Very difficult")]
        VeryDifficult = 4
    }
}