using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public class StatementRiskType
    {
        public short StatementRiskTypeId { get; set; }

        public short? ParentRiskTypeId { get; set; }

        public virtual StatementRiskType ParentRiskType { get; set; }

        public RiskCategories Category { get; set; }

        public string Description { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;
    }

    public enum RiskCategories
    {
        Unknown,
        RiskArea,
        Location
    }
}
