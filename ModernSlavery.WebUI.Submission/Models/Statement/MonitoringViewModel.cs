using AutoMapper;
using ModernSlavery.Core.Entities.StatementSummary;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class MonitoringViewModelMapperProfile : Profile
    {
        public MonitoringViewModelMapperProfile()
        {
            CreateMap<StatementSummary1, MonitoringViewModel>();

            CreateMap<MonitoringViewModel, StatementSummary1>(MemberList.Source)
                .ForMember(d => d.OtherWorkConditionsMonitoring, opt => opt.MapFrom(s=>s.OtherWorkConditionsMonitoring))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class MonitoringViewModel : BaseViewModel
    {
        public override string PageTitle => "Do you want to tell us about any other ways you monitored working conditions across your organisation and supply chain?";

        [MaxLength(1024)]
        public string OtherWorkConditionsMonitoring { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (!string.IsNullOrWhiteSpace(OtherWorkConditionsMonitoring)) return Status.Complete;
            return Status.Incomplete;
        }
    }
}
