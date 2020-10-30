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

            CreateMap<ComplianceViewModel, StatementModel>(MemberList.Source)                
                .ForMember(d => d.StructureDetails, opt => opt.MapFrom(s=>s.StructureDetails))
                .ForMember(d => d.IncludesPolicies, opt => opt.MapFrom(s=>s.IncludesPolicies))
                .ForMember(d => d.PolicyDetails, opt => opt.MapFrom(s=>s.PolicyDetails))
                .ForMember(d => d.IncludesRisks, opt => opt.MapFrom(s=>s.IncludesRisks))
                .ForMember(d => d.RisksDetails, opt => opt.MapFrom(s=>s.RisksDetails))
                .ForMember(d => d.IncludesDueDiligence, opt => opt.MapFrom(s=>s.IncludesDueDiligence))
                .ForMember(d => d.DueDiligenceDetails, opt => opt.MapFrom(s=>s.DueDiligenceDetails))
                .ForMember(d => d.IncludesTraining, opt => opt.MapFrom(s=>s.IncludesTraining))
                .ForMember(d => d.TrainingDetails, opt => opt.MapFrom(s=>s.TrainingDetails))
                .ForMember(d => d.IncludesGoals, opt => opt.MapFrom(s=>s.IncludesGoals))
                .ForMember(d => d.GoalsDetails, opt => opt.MapFrom(s=>s.GoalsDetails))                
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }
    public class ComplianceViewModel : BaseViewModel
    {
        public override string PageTitle => "Areas covered by your modern slavery statement";

        public bool? IncludesStructure { get; set; }
        [MaxLength(128)]
        public string StructureDetails { get; set; }

        public bool? IncludesPolicies { get; set; }
        [MaxLength(128)]
        public string PolicyDetails { get; set; }

        public bool? IncludesRisks { get; set; }
        [MaxLength(128)]
        public string RisksDetails { get; set; }

        public bool? IncludesDueDiligence { get; set; }
        [MaxLength(128)]
        public string DueDiligenceDetails { get; set; }

        public bool? IncludesTraining { get; set; }
        [MaxLength(128)]
        public string TrainingDetails { get; set; }

        public bool? IncludesGoals { get; set; }
        [MaxLength(128)]
        public string GoalsDetails { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (IncludesStructure == false && string.IsNullOrWhiteSpace(StructureDetails))
                validationResults.AddValidationError(3200, nameof(StructureDetails));

            if (IncludesPolicies == false && string.IsNullOrWhiteSpace(PolicyDetails))
                validationResults.AddValidationError(3200, nameof(PolicyDetails));

            if (IncludesRisks == false && string.IsNullOrWhiteSpace(RisksDetails))
                validationResults.AddValidationError(3200, nameof(RisksDetails));

            if (IncludesDueDiligence == false && string.IsNullOrWhiteSpace(DueDiligenceDetails))
                validationResults.AddValidationError(3200, nameof(DueDiligenceDetails));

            if (IncludesTraining == false && string.IsNullOrWhiteSpace(TrainingDetails))
                validationResults.AddValidationError(3200, nameof(TrainingDetails));

            if (IncludesGoals == false && string.IsNullOrWhiteSpace(GoalsDetails))
                validationResults.AddValidationError(3200, nameof(GoalsDetails));

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (IncludesStructure.HasValue
                && ((IncludesStructure == true) || !string.IsNullOrWhiteSpace(StructureDetails))
                && IncludesPolicies.HasValue
                && ((IncludesPolicies == true) || !string.IsNullOrWhiteSpace(PolicyDetails))
                && IncludesRisks.HasValue
                && ((IncludesRisks == true) || !string.IsNullOrWhiteSpace(RisksDetails))
                && IncludesDueDiligence.HasValue
                && ((IncludesDueDiligence == true) || !string.IsNullOrWhiteSpace(DueDiligenceDetails))
                && IncludesTraining.HasValue
                && ((IncludesTraining == true) || !string.IsNullOrWhiteSpace(TrainingDetails))
                && IncludesGoals.HasValue
                && ((IncludesGoals == true) || !string.IsNullOrWhiteSpace(GoalsDetails))) return Status.Complete;

            return Status.Incomplete;
        }
    }
}
