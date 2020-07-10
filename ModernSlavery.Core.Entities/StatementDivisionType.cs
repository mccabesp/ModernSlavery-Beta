using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    //TODO: Rename to StatementTrainingType
    public class StatementDivisionType
    {
        //TODO: Rename to StatementTrainingTypeId
        public short StatementDivisionTypeId { get; set; }

        public string Description { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;
    }
}
