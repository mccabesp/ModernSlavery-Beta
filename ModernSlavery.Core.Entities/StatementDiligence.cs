using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public class StatementDiligence
    {
        public long StatementDiligenceId { get; set; }

        public short StatementDiligenceTypeId { get; set; }

        public virtual StatementDiligenceType StatementDiligenceType { get; set; }

        public long StatementId { get; set; }

        public virtual Statement Statement { get; set; }

        //TODO: If have a new StatementDiligenceParentTypeId (see StatementDiligenceType entity) we can then store the titles of "Other" due diligences into this Description field. 
        //TODO: public string Details { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;
    }
}
