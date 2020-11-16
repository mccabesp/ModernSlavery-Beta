using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using System.Linq;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Binding;
using ModernSlavery.Core.Classes.StatementTypeIndexes;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class SectorsViewModelMapperProfile : Profile
    {
        public SectorsViewModelMapperProfile()
        {
            CreateMap<StatementModel, SectorsViewModel>();

            CreateMap<SectorsViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForMember(d => d.Sectors, opt => opt.MapFrom(s => s.Sectors))
                .ForMember(d => d.OtherSectors, opt => opt.MapFrom(s => s.OtherSectors));
        }
    }

    [DependencyModelBinder]
    public class SectorsViewModel : BaseViewModel
    {
        [IgnoreMap]
        [Newtonsoft.Json.JsonIgnore]//This needs to be Newtonsoft.Json.JsonIgnore namespace not System.Text.Json.Serialization.JsonIgnore
        public SectorTypeIndex SectorTypes { get; }
        public SectorsViewModel(SectorTypeIndex sectorTypes)
        {
            SectorTypes = sectorTypes;
        }

        public SectorsViewModel()
        {

        }

        public override string PageTitle => "Which sector(s) does your organisation operate in?";

        public List<short> Sectors { get; set; } = new List<short>();

        [MaxLength(128)]//We need at least one validation annotation otherwise Validate wont execute
        public string OtherSectors { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            var otherId = SectorTypes.Single(x => x.Description.Equals("Other")).Id;

            if (Sectors.Contains(otherId) && string.IsNullOrEmpty(OtherSectors))
                validationResults.AddValidationError(3500, nameof(OtherSectors));

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (Sectors.Any())
            {
                var other = SectorTypes.Single(x => x.Description.Equals("Other"));

                if (Sectors.Any(t => t == other.Id) && string.IsNullOrWhiteSpace(OtherSectors)) return Status.InProgress;

                else return Status.Complete;
            }

            return Status.Incomplete;
        }
    }
}
