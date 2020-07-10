using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    //TODO: Rename to StatementTrainingType
    public class StatementDiligenceType
    {
        public short StatementDiligenceTypeId { get; set; }

        //TODO: Create a new StatementDiligenceParentTypeId so we can havea hierarchy of Due diligence categories
        //public short? StatementDiligenceParentTypeId { get; set; }

        //TODO: We will have a Diligence type call "Other"
        public string Description { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;
    }
}
