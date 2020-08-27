using System;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.WebUI.Submission.Models
{
    [Serializable]
    public class ScopeViewModel
    {
        public long OrganisationScopeId { get; set; }
        public ScopeStatuses ScopeStatus { get; set; }
        public DateTime StatusDate { get; set; }
        public DateTime DeadlineDate { get; set; }
        public long LatestReturnId { get; set; }
        public RegisterStatuses RegisterStatus { get; set; }
    }
}