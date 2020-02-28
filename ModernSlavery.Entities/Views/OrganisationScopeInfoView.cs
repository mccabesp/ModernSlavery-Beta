using System;
using System.Collections.Generic;

namespace ModernSlavery.Entities
{
    public partial class OrganisationScopeInfoView
    {
        public long OrganisationId { get; set; }
        public string ScopeStatus { get; set; }
        public DateTime ScopeStatusDate { get; set; }
        public string RegisterStatus { get; set; }
        public DateTime RegisterStatusDate { get; set; }
        public int? SnapshotYear { get; set; }
    }
}
