using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public class StatementDiligence
    {
        public long StatementDiligenceId { get; set; }

        public short StatementDiligenceTypeId { get; set; }

        public virtual StatmentDiligenceType StatmentDiligenceType { get; set; }

        public long StatmentId { get; set; }

        public virtual StatementMetadata Statement { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;
    }
}
