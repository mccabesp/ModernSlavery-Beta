using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public class StatementRisk
    {
        public short StatementRiskId { get; set; }

        public short StatementRiskTypeId { get; set; }

        public virtual StatementRiskType StatementRiskType { get; set; }

        //TODO: Create new Details field.
        //NOTE: This will allow us to add further details on risks if required and also use for "Other" risk categories (eg., 'Other vulnerable groups')
        //TODO: public string Details { get; set; }

        public long StatementId { get; set; }



        public virtual Statement Statement { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;
    }
}
