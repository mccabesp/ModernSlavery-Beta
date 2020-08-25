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
        SubmittedAStatement = 0,
        Viewed1OrMoreStatements = 1,
        SubmittedAndViewedStatements = 2,

    }

    public enum DifficultyTypes : byte
    {
        VeryEasy = 0,
        Easy = 1,
        Neutral = 2,
        Difficult = 3,
        VeryDifficult = 4
    }


}