using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using System.Linq;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Binding;
using ModernSlavery.Core.Classes.StatementTypeIndexes;
using static ModernSlavery.Core.Entities.Statement;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class SectorPageViewModelMapperProfile : Profile
    {
        public SectorPageViewModelMapperProfile()
        {
            CreateMap<StatementModel, SectorPageViewModel>();

            CreateMap<SectorPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.Sectors, opt => opt.MapFrom(s => s.Sectors))
                .ForMember(d => d.OtherSectors, opt => opt.MapFrom(s => s.OtherSector))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    [DependencyModelBinder]
    public class SectorPageViewModel : BaseViewModel
    {
        [IgnoreMap]
        [Newtonsoft.Json.JsonIgnore]//This needs to be Newtonsoft.Json.JsonIgnore namespace not System.Text.Json.Serialization.JsonIgnore
        public SectorTypeIndex SectorTypes { get; }
        public SectorPageViewModel(SectorTypeIndex sectorTypes)
        {
            SectorTypes = sectorTypes;
        }

        public SectorPageViewModel()
        {

        }

        public override string PageTitle => "Which sector(s) does your organisation operate in?";

        public List<short> Sectors { get; set; } = new List<short>();

        [MaxLength(128)]//We need at least one validation annotation otherwise Validate wont execute
        public string OtherSector { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            var otherId = SectorTypes.Single(x => x.Description.Equals("Other")).Id;

            if (Sectors.Contains(otherId) && string.IsNullOrEmpty(OtherSector))
                validationResults.AddValidationError(3300, nameof(OtherSector));

            return validationResults;
        }

        public override bool IsComplete()
        {
            var other = SectorTypes.Single(x => x.Description.Equals("Other"));

            return Sectors.Any()
                && (!Sectors.Any(t => t == other.Id || !string.IsNullOrWhiteSpace(OtherSector)));
        }
    }
}
