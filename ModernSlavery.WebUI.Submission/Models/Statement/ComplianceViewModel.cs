using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.WebUI.Shared.Classes.Extensions;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class ComplianceViewModelMapperProfile : Profile
    {
        public ComplianceViewModelMapperProfile()
        {
            CreateMap<StatementModel, ComplianceViewModel>();

            CreateMap<ComplianceViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForMember(d => d.StructureDetails, opt => opt.MapFrom(s => s.StructureDetails))
                .ForMember(d => d.IncludesPolicies, opt => opt.MapFrom(s => s.IncludesPolicies))
                .ForMember(d => d.PolicyDetails, opt => opt.MapFrom(s => s.PolicyDetails))
                .ForMember(d => d.IncludesRisks, opt => opt.MapFrom(s => s.IncludesRisks))
                .ForMember(d => d.RisksDetails, opt => opt.MapFrom(s => s.RisksDetails))
                .ForMember(d => d.IncludesDueDiligence, opt => opt.MapFrom(s => s.IncludesDueDiligence))
                .ForMember(d => d.DueDiligenceDetails, opt => opt.MapFrom(s => s.DueDiligenceDetails))
                .ForMember(d => d.IncludesTraining, opt => opt.MapFrom(s => s.IncludesTraining))
                .ForMember(d => d.TrainingDetails, opt => opt.MapFrom(s => s.TrainingDetails))
                .ForMember(d => d.IncludesGoals, opt => opt.MapFrom(s => s.IncludesGoals))
                .ForMember(d => d.GoalsDetails, opt => opt.MapFrom(s => s.GoalsDetails));
        }
    }
    public class ComplianceViewModel : BaseStatementViewModel
    {
        public override string PageTitle => "Does your statement cover the following areas in relation to modern slavery?";

        public bool? IncludesStructure { get; set; }
        [MaxLength(200)]
        public string StructureDetails { get; set; }

        public bool? IncludesPolicies { get; set; }
        [MaxLength(200)]
        public string PolicyDetails { get; set; }

        public bool? IncludesRisks { get; set; }
        [MaxLength(200)]
        public string RisksDetails { get; set; }

        public bool? IncludesDueDiligence { get; set; }
        [MaxLength(200)]
        public string DueDiligenceDetails { get; set; }

        public bool? IncludesTraining { get; set; }
        [MaxLength(200)]
        public string TrainingDetails { get; set; }

        public bool? IncludesGoals { get; set; }
        [MaxLength(200)]
        public string GoalsDetails { get; set; }

        public override Status GetStatus()
        {
            if (IncludesStructure.HasValue
                && IncludesPolicies.HasValue
                && IncludesRisks.HasValue
                && IncludesDueDiligence.HasValue
                && IncludesTraining.HasValue
                && IncludesGoals.HasValue) return Status.Complete;

            else if (!IncludesStructure.HasValue
                && !IncludesPolicies.HasValue
                && !IncludesRisks.HasValue
                && !IncludesDueDiligence.HasValue
                && !IncludesTraining.HasValue
                && !IncludesGoals.HasValue) return Status.Incomplete;

            else return Status.InProgress;
        }
    }
}
