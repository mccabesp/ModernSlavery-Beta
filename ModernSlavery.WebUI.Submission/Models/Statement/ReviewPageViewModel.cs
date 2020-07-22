using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class ReviewPageViewModelMapperProfile : Profile
    {
        public ReviewPageViewModelMapperProfile()
        {
            CreateMap<StatementModel,ReviewPageViewModel>();
            CreateMap<ReviewPageViewModel, StatementModel>(MemberList.Source);
        }
    }

    public class ReviewPageViewModel : BaseViewModel
    {
        public int ReportingDeadlineYear { get; set; }
        public string OrganisationName { get; set; }

        public bool YourStatementCompleted { get; set; }
        public bool ComplianceCompleted { get; set; }

        public bool YourOrganisationCompleted { get; set; }
        public bool PoliciesCompleted { get; set; }
        public bool RisksCompleted { get; set; }
        public bool DueDiligencCompleted { get; set; }
        public bool TrainingCompleted { get; set; }
        public bool ProgressCompleted { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new System.NotImplementedException();
        }

    }
}
