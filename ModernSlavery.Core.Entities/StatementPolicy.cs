using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public class StatementPolicy
    {
        public short StatementPolicyTypeId { get; set; }

        public virtual StatementPolicyType StatementPolicyType { get; set; }

        public long StatementId { get; set; }

        public virtual Statement Statement { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;
    }
}
