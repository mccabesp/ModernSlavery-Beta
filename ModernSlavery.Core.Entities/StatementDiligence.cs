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

        public virtual StatementDiligenceType StatementDiligenceType { get; set; }

        public long StatementId { get; set; }

        public virtual StatementMetadata Statement { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;
    }
}
