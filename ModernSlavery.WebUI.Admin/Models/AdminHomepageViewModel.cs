using System;

namespace ModernSlavery.WebUI.Admin.Models
{
    public class AdminHomepageViewModel
    {
        public int FeedbackCount { get; set; }
        public DateTime? LatestFeedbackDate { get; set; }
    }
}