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
            StatementRelevantRisks = new HashSet<StatementRelevantRisk>();
            StatementHighRisks = new HashSet<StatementHighRisk>();
            StatementLocationRisks = new HashSet<StatementLocationRisk>();
        }

        public short StatementRiskTypeId { get; set; }

        public short? ParentRiskTypeId { get; set; }

        public RiskCategories Category { get; set; }

        public string Description { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public virtual StatementRiskType ParentRiskType { get; set; }
        public virtual ICollection<StatementRiskType> ChildRiskType { get; set; }
        public virtual ICollection<StatementRelevantRisk> StatementRelevantRisks { get; set; }
        public virtual ICollection<StatementHighRisk> StatementHighRisks { get; set; }
        public virtual ICollection<StatementLocationRisk> StatementLocationRisks { get; set; }
    }

    public enum RiskCategories : byte
    {
        Unknown=0,
        RiskArea=1,
        Location=2
    }
}
