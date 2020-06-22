using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public class StatementOrganisation
    {
        public long StatementOrganisationId { get; set; }

        public long StatementId { get; set; }

        public virtual StatementMetadata Statement { get; set; }

        public long? OrganisationId { get; set; }

        public virtual Organisation Organisation { get; set; }

        public bool Included { get; set; }

        public string OrganisationName { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;
    }
}
