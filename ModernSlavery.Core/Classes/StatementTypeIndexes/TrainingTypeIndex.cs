using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace ModernSlavery.Core.Classes.StatementTypeIndexes
{
    public class TrainingTypeIndex : List<TrainingTypeIndex.TrainingType>
    {
        public TrainingTypeIndex(IDataRepository dataRepository) : base()
        {
            var types = dataRepository.GetAll<StatementTrainingType>().Select(t => new TrainingType { Id = t.StatementTrainingTypeId, Description = t.Description });
            AddRange(types);
        }

        public TrainingTypeIndex() { }

        public class TrainingType
        {
            public short Id { get; set; }
            public string Description { get; set; }
        }

    }
}
