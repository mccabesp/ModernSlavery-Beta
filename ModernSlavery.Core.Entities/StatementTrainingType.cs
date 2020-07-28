using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public class StatementTrainingType
    {
        public StatementTrainingType()
        {
            StatementTraining = new HashSet<StatementTraining>();
        }

        public short StatementTrainingTypeId { get; set; }

        public string Description { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public virtual ICollection<StatementTraining> StatementTraining { get; set; }

    }
}
