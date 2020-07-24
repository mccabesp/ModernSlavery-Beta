using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using TrainingType = ModernSlavery.WebUI.Submission.Classes.TrainingTypeIndex.TrainingType;

namespace ModernSlavery.WebUI.Submission.Classes
{
    public class TrainingTypeIndex : List<TrainingType>
    {
        public TrainingTypeIndex(IDataRepository dataRepository):base()
        {
            var types = dataRepository.GetAll<StatementTrainingType>().Select(t => new TrainingType { Id=t.StatementTrainingTypeId, Description = t.Description });
            this.AddRange(types);
        }

        public class TrainingType
        {
            public short Id { get; set; }
            public string Description { get; set; }
        }

    }
}
