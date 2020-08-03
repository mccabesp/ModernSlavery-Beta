using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using RiskType = ModernSlavery.WebUI.Submission.Classes.RiskTypeIndex.RiskType;

namespace ModernSlavery.WebUI.Submission.Classes
{
    public class RiskTypeIndex : List<RiskType>
    {
        public RiskTypeIndex(IDataRepository dataRepository) : base()
        {
            var types = dataRepository.GetAll<StatementRiskType>()
                .Select(t => new RiskType { Id = t.StatementRiskTypeId, ParentId = t.ParentRiskTypeId, Category = t.Category.ToString(), Description = t.Description })
                .AsEnumerable();
            var countries = types.Where(t => t.Category == RiskCategories.Location.ToString())
                .OrderBy(t => t.Description);
            this.AddRange(types.Where(t => t.Category != RiskCategories.Location.ToString()));
            this.AddRange(countries);
            Regions = this.Where(t => t.ParentId == null && t.Category == RiskCategories.Location.ToString()).ToList();
        }

        public RiskTypeIndex() { }

        public class RiskType
        {
            public short Id { get; set; }
            public short? ParentId { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
        }
        public readonly IList<RiskType> Regions;
   }
}
