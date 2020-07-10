using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    //TODO: Rename to StatementTraining
    public class StatementTrainingDivision
    {
        //TODO: Rename to StatementTrainingTypeId
        public short StatementDivisionTypeId { get; set; }

        //TODO: Rename to StatementTrainingType
        public virtual StatementDivisionType StatementDivisionType { get; set; }

        public long StatementId { get; set; }

        public virtual Statement Statement { get; set; }

        public DateTime Created { get; set; }
    }
}
