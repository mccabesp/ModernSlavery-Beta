using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using static ModernSlavery.Core.Classes.StatementTypeIndexes.SectorTypeIndex;

namespace ModernSlavery.Core.Classes.StatementTypeIndexes
{
    public class SectorTypeIndex : List<SectorType>
    {
        public readonly IDataRepository _dataRepository;
        public SectorTypeIndex(IDataRepository dataRepository) : base()
        {
            _dataRepository = dataRepository;
            Load();
        }
        public SectorTypeIndex() { }

        public class SectorType
        {
            public short Id { get; set; }
            public string Description { get; set; }
        }

        public void Load()
        {
            Clear();
            var types = _dataRepository.GetAll<StatementSectorType>()
                .OrderBy(t => t.Description)
                .Select(t => new SectorType { Id = t.StatementSectorTypeId, Description = t.Description })
                .AsEnumerable();

            if (types.Any())
            {
                var other = types.Single(t => t.Description == "Other");
                AddRange(types.Where(t => t.Id != other.Id));
                Add(other);
            }
        }
    }
}
