using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using PolicyType = ModernSlavery.WebUI.Submission.Classes.PolicyTypeIndex.PolicyType;

namespace ModernSlavery.WebUI.Submission.Classes
{
    public class PolicyTypeIndex : List<PolicyType>
    {
        public PolicyTypeIndex(IDataRepository dataRepository):base()
        {
            var types = dataRepository.GetAll<StatementPolicyType>().Select(t => new PolicyType { Id=t.StatementPolicyTypeId, Description = t.Description });
            this.AddRange(types);
        }

        public class PolicyType
        {
            public short Id { get; set; }
            public string Description { get; set; }
        }

    }
}
