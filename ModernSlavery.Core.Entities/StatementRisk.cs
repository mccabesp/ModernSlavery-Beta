using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public class StatementRisk
    {
        public short StatementRiskId { get; set; }

        public long StatementId { get; set; }

        public virtual StatementMetadata Statement { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;
    }
}
