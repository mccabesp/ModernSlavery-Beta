using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace ModernSlavery.Core.Classes.StatementTypeIndexes
{
    public class PolicyTypeIndex : List<PolicyTypeIndex.PolicyType>
    {
        public PolicyTypeIndex(IDataRepository dataRepository) : base()
        {
            var types = dataRepository.GetAll<StatementPolicyType>().Select(t => new PolicyType { Id = t.StatementPolicyTypeId, Description = t.Description });
            AddRange(types);
        }

        public PolicyTypeIndex() { }

        public class PolicyType
        {
            public short Id { get; set; }
            public string Description { get; set; }
        }

    }
}
