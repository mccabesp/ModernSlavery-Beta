using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public class StatementSectorType
    {
        public StatementSectorType()
        {
            StatementSectors = new HashSet<StatementSector>();
        }

        public short StatementSectorTypeId { get; set; }

        public string Description { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;
        public virtual ICollection<StatementSector> StatementSectors { get; set; }

    }
}
