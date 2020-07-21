using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class CompliancePageViewModelMapperProfile : Profile
    {
        public CompliancePageViewModelMapperProfile()
        {
            CreateMap<StatementModel, CompliancePageViewModel>();
            CreateMap<CompliancePageViewModel, StatementModel>(MemberList.Source);
        }
    }

    public class CompliancePageViewModel: BaseViewModel
    {
        public bool? IncludesStructure { get; set; }
        public string StructureDetails { get; set; }

        public bool? IncludesPolicies { get; set; }
        public string PolicyDetails { get; set; }

        public bool? IncludesRisks { get; set; }
        public string RisksDetails { get; set; }

        public bool? IncludesDueDiligence { get; set; }
        public string DueDiligenceDetails { get; set; }

        public bool? IncludesTraining { get; set; }
        public string TrainingDetails { get; set; }

        public bool? IncludesGoals { get; set; }
        public string GoalsDetails { get; set; }
    }
}
