using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public class StatementDiligenceType
    {
        public short StatementDiligenceTypeId { get; set; }

        public short? ParentDiligenceTypeId { get; set; }

        public virtual StatementDiligenceType StatementDiligenceParent { get; set; }

        public string Description { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;
    }
}
