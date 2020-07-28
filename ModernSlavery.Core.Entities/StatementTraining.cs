using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public class StatementTraining
    {
        public short StatementTrainingTypeId { get; set; }

        public virtual StatementTrainingType StatementTrainingType { get; set; }

        public long StatementId { get; set; }

        public virtual Statement Statement { get; set; }

        public DateTime Created { get; set; }

        // Details for other selection
        public string Details { get; set; }
    }
}
