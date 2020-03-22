using System;
using System.Collections.Generic;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Entities
{
    [Serializable]
    public class OrganisationPublicSectorType
    {
        public long OrganisationPublicSectorTypeId { get; set; }

        public int PublicSectorTypeId { get; set; }

        public long OrganisationId { get; set; }

        public string Source { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public DateTime? Retired { get; set; }

        public virtual PublicSectorType PublicSectorType { get; set; }

        public virtual ICollection<Organisation> Organisations { get; set; }
    }
}