using AutoMapper;
using ModernSlavery.Core.Entities.StatementSummary;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class ProgressPageViewModelMapperProfile : Profile
    {
        public ProgressPageViewModelMapperProfile()
        {
            CreateMap<StatementSummary1, MonitoringProgressPageViewModel>();

            CreateMap<MonitoringProgressPageViewModel, StatementSummary1>(MemberList.Source)
                .ForMember(d => d.ProgressMeasures, opt => opt.MapFrom(s=>s.ProgressMeasures))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class MonitoringProgressPageViewModel : BaseViewModel
    {
        public override string PageTitle => "How does your statement demonstrate your progress over time in addressing modern slavery risks?";

        [MaxLength(1024)]
        public string ProgressMeasures { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            return validationResults;
        }

        public override bool IsComplete()
        {
            return !string.IsNullOrWhiteSpace(ProgressMeasures);
        }
    }
}
