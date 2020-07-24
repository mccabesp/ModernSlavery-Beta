using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using DiligenceType = ModernSlavery.WebUI.Submission.Classes.DiligenceTypeIndex.DiligenceType;

namespace ModernSlavery.WebUI.Submission.Classes
{
    public class DiligenceTypeIndex : List<DiligenceType>
    {
        public DiligenceTypeIndex(IDataRepository dataRepository):base()
        {
            var types = dataRepository.GetAll<StatementDiligenceType>().Select(t => new DiligenceType { Id=t.StatementDiligenceTypeId, ParentId = t.ParentDiligenceTypeId, Description = t.Description });
            this.AddRange(types);
        }

        public class DiligenceType
        {
            public short Id { get; set; }
            public short? ParentId { get; set; }
            public string Description { get; set; }
        }

    }
}
