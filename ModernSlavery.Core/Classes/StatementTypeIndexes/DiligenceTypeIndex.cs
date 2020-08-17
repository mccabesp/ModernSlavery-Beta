using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace ModernSlavery.Core.Classes.StatementTypeIndexes
{
    public class DiligenceTypeIndex : List<DiligenceTypeIndex.DiligenceType>
    {
        public DiligenceTypeIndex(IDataRepository dataRepository) : base()
        {
            var types = dataRepository.GetAll<StatementDiligenceType>().Select(t => new DiligenceType { Id = t.StatementDiligenceTypeId, ParentId = t.ParentDiligenceTypeId, Description = t.Description });
            AddRange(types);
        }

        public DiligenceTypeIndex() { }

        public class DiligenceType
        {
            public short Id { get; set; }
            public short? ParentId { get; set; }
            public string Description { get; set; }
        }

    }
}
