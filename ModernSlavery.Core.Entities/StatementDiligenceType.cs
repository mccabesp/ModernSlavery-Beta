using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public class StatementDiligenceType
    {
        public StatementDiligenceType()
        {
            ChildDiligenceTypes = new HashSet<StatementDiligenceType>();
        }

        public short StatementDiligenceTypeId { get; set; }

        public short? ParentDiligenceTypeId { get; set; }

        public string Description { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public virtual StatementDiligenceType ParentDiligenceType { get; set; }

        public virtual ICollection<StatementDiligenceType> ChildDiligenceTypes { get; set; }
    }
}
