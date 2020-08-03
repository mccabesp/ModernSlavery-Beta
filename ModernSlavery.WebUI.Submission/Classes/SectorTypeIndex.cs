using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using SectorType = ModernSlavery.WebUI.Submission.Classes.SectorTypeIndex.SectorType;

namespace ModernSlavery.WebUI.Submission.Classes
{
    public class SectorTypeIndex : List<SectorType>
    {
        public SectorTypeIndex(IDataRepository dataRepository):base()
        {
            var types = dataRepository.GetAll<StatementSectorType>()
                .OrderBy(t => t.Description)
                .Select(t => new SectorType { Id = t.StatementSectorTypeId, Description = t.Description })
                .AsEnumerable();
            var other = types.Single(t => t.Description == "Other");
            this.AddRange(types.Where(t => t.Id != other.Id));
            this.Add(other);
        }
        public SectorTypeIndex() { }

        public class SectorType
        {
            public short Id { get; set; }
            public string Description { get; set; }
        }

    }
}
