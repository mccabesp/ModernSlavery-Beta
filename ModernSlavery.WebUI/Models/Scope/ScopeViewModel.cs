using System;
using ModernSlavery.WebUI.Shared.Models;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Abstractions;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.SharedKernel;
using ModernSlavery.Entities;
using ModernSlavery.Entities.Enums;
using ModernSlavery.WebUI.Shared.Models;

namespace ModernSlavery.WebUI.Models.Scope
{
    [Serializable]
    public class ScopeViewModel
    {

        public long OrganisationScopeId { get; set; }
        public ScopeStatuses ScopeStatus { get; set; }
        public DateTime StatusDate { get; set; }
        public DateTime SnapshotDate { get; set; }
        public long LatestReturnId { get; set; }
        public RegisterStatuses RegisterStatus { get; set; }

    }
}
