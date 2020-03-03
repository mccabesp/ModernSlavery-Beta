using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Entities.Enums;

namespace ModernSlavery.WebUI.Models.Organisation
{
    [Serializable]
    public class DeclareScopeModel
    {

        public string OrganisationName { get; set; }
        public DateTime SnapshotDate { get; set; }

        [Required(AllowEmptyStrings = false)]
        public ScopeStatuses? ScopeStatus { get; set; }

    }
}
