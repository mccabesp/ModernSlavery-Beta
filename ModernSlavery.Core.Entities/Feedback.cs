using ModernSlavery.Core.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace ModernSlavery.Core.Entities
{
    public class Feedback
    {
        public long FeedbackId { get; set; }

        #region WhyVisitMSUSite
        public WhyVisitMSUSite? WhyVisitMSUSite { get; set; }

        #endregion

        #region DifficultyTypes

        public DifficultyTypes? Difficulty { get; set; }

        #endregion

        public string Details { get; set; }

        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }

        public DateTime CreatedDate { get; set; } = VirtualDateTime.Now;

    }
    public enum WhyVisitMSUSite : byte
    {
        Unknown = 0,
        SubmittedAStatement = 1,
        Viewed1OrMoreStatements = 2,
        SubmittedAndViewedStatements = 3,

    }

    public enum DifficultyTypes : byte
    {
        Unknown = 0,
        VeryEasy = 1,
        Easy = 2,
        Neutral = 3,
        Difficult = 4,
        VeryDifficult = 5
    }


}