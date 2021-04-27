using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.GDSDesignSystem;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Shared.Models
{
    public class FeedbackViewModel : GovUkViewModel
    {
        [Required]
        public WhyVisitSite? WhyVisitMSUSite { get; set; }

        [Required]
        public HowEasyIsThisServiceToUse? HowEasyIsThisServiceToUse { get; set; }

        [MaxLength(2000)]
        [Text]
        public string Details { get; set; }

        [EmailAddress]
        [MaxLength(254)]
        public string EmailAddress { get; set; }
        
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [RelativeUrl]
        public string BackUrl { get; set; }
    }
    public enum WhyVisitSite : byte
    {
        //Unknown
        Unknown = 0,

        [Description("Submitted a statement")]
        SubmittedAStatement = 1,

        [Description("Viewed 1 or more statements")]
        Viewed1OrMoreStatements = 2,

        [Description("Submitted and viewed statements")]
        SubmittedAndViewedStatements = 3,
    }

    public enum HowEasyIsThisServiceToUse : byte
    {
        //Unknown
        Unknown = 0,

        [Description("Very easy")]
        VeryEasy = 1,

        [Description("Easy")]
        Easy = 2,

        [Description("Neither easy nor difficult")]
        Neutral = 3,

        [Description("Difficult")]
        Difficult = 4,

        [Description("Very difficult")]
        VeryDifficult = 5
    }
}