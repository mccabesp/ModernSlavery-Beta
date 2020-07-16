using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public class StatementRiskType
    {
        public StatementRiskType()
        {
            ChildRiskType = new HashSet<StatementRiskType>();
        }

        public short StatementRiskTypeId { get; set; }

        public short? ParentRiskTypeId { get; set; }

        public RiskCategories Category { get; set; }

        public string Description { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public virtual StatementRiskType ParentRiskType { get; set; }
        public virtual ICollection<StatementRiskType> ChildRiskType { get; set; }
    }

    public enum RiskCategories : byte
    {
        Unknown=0,
        RiskArea=1,
        Location=2
    }
}
