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
        [Required]
        public WhyVisitSite? WhyVisitMSUSite { get; set; }

        [Required]
        public HowEasyIsThisServiceToUse? HowEasyIsThisServiceToUse { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Details { get; set; }
        [EmailAddress]
        public string EmailAddress { get; set; }
        [Phone]
        public string PhoneNumber { get; set; }
    }
    public enum WhyVisitSite : byte
    {
        [Display(Description = "Submitted a statement")]
        SubmittedAStatement = 0,

        [Display(Description = "Viewed 1 or more statements")]
        Viewed1OrMoreStatements = 1,

        [Display(Description = "Submitted and viewed statements")]
        SubmittedAndViewedStatements = 2,


    }

    public enum HowEasyIsThisServiceToUse : byte
    {
        [Display(Description = "Very easy")]
        VeryEasy = 0,

        [Display(Description = "Easy")]
        Easy = 1,

        [Display(Description = "Neither easy nor difficult")]
        Neutral = 2,

        [Display(Description = "Difficult")]
        Difficult = 3,

        [Display(Description = "Very difficult")]
        VeryDifficult = 4
    }
}