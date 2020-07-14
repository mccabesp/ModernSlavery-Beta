using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public class StatementTraining
    {
        public short StatementDivisionTypeId { get; set; }

        public virtual StatementDivisionType StatementTrainingType { get; set; }

        public long StatementId { get; set; }

        public virtual Statement Statement { get; set; }

        public DateTime Created { get; set; }
    }
}
