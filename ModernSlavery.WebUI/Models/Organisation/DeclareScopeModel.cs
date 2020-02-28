using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.WebUI.Shared.Models;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Abstractions;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.SharedKernel;
using ModernSlavery.Entities;
using ModernSlavery.Entities.Enums;
using ModernSlavery.WebUI.Shared.Models;

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
