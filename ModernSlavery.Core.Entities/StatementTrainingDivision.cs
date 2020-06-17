using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public class StatementTrainingDivision
    {
        public short StatmentDivisionTypeId { get; set; }

        public virtual StatementDivisionType StatementDivisionType { get; set; }

        public long StatementId { get; set; }

        public virtual StatementMetadata Statement { get; set; }

        public DateTime Created { get; set; }
    }
}
