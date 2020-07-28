using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public class StatementPolicyType
    {
        public StatementPolicyType()
        {
            StatementPolicies = new HashSet<StatementPolicy>();
        }

        public short StatementPolicyTypeId { get; set; }

        public string Description { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public virtual ICollection<StatementPolicy> StatementPolicies { get; set; }

    }
}
