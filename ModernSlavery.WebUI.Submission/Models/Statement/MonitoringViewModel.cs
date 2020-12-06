using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
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
            CreateMap<StatementModel, MonitoringViewModel>()
                .ForMember(d => d.OtherWorkConditionsMonitoring, opt => opt.MapFrom(s => s.Summary.OtherWorkConditionsMonitoring));

            CreateMap<MonitoringViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForPath(d => d.Summary.OtherWorkConditionsMonitoring, opt => opt.MapFrom(s => s.OtherWorkConditionsMonitoring));
        }
    }

    public class MonitoringViewModel : BaseStatementViewModel
    {
        public override string PageTitle => "Are there any other ways you monitored working conditions across your operations and supply chains?";

        [MaxLength(200)]
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
