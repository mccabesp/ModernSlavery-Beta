using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.WebUI.Submission.Models
{
    [Serializable]
    public class DeclareScopeModel
    {
        public string OrganisationName { get; set; }
        public DateTime ReportingDeadline { get; set; }

        [Required(AllowEmptyStrings = false)] public ScopeStatuses? ScopeStatus { get; set; }
    }
}