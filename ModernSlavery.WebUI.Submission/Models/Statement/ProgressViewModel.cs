using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Entities.StatementSummary;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class ProgressViewModelMapperProfile : Profile
    {
        public ProgressViewModelMapperProfile()
        {
            CreateMap<StatementModel, ProgressViewModel>()
                .ForMember(d => d.ProgressMeasures, opt => opt.MapFrom(s => s.Summary.ProgressMeasures));

            CreateMap<ProgressViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForPath(d => d.Summary.ProgressMeasures, opt => opt.MapFrom(s => s.ProgressMeasures));
        }
    }

    public class ProgressViewModel : BaseStatementViewModel
    {
        public override string PageTitle => "How does your statement demonstrate your progress over time in addressing modern slavery risks?";

        [MaxLength(500)]
        [Text] 
        public string ProgressMeasures { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (!string.IsNullOrWhiteSpace(ProgressMeasures)) return Status.Complete;
            return Status.Incomplete;
        }

    }
}
