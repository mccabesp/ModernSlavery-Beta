using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.BusinessDomain.Shared.Models;
using System;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class CompliancePageViewModelMapperProfile : Profile
    {
        public CompliancePageViewModelMapperProfile()
        {
            CreateMap<StatementModel, CompliancePageViewModel>()
                .ForMember(s => s.BackUrl, opt => opt.Ignore())
                .ForMember(s => s.CancelUrl, opt => opt.Ignore())
                .ForMember(s => s.ContinueUrl, opt => opt.Ignore());

            CreateMap<CompliancePageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForSourceMember(s => s.PageTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SubTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ReportingDeadlineYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.BackUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.CancelUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ContinueUrl, opt => opt.DoNotValidate());
        }
    }
    public class CompliancePageViewModel : BaseViewModel
    {
        public override string PageTitle => "Areas covered by your modern slavery statement";

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

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IncludesStructure == false && string.IsNullOrWhiteSpace(StructureDetails))
                yield return new ValidationResult("Please provide the detail for: Your organisatio's structure, business and supply chains");

            if (IncludesPolicies == false && string.IsNullOrWhiteSpace(PolicyDetails))
                yield return new ValidationResult("Please provide the detail for: Policies");

            if (IncludesRisks == false && string.IsNullOrWhiteSpace(RisksDetails))
                yield return new ValidationResult("Please provide the detail for: Risk assessment and management");

            if (IncludesDueDiligence == false && string.IsNullOrWhiteSpace(DueDiligenceDetails))
                yield return new ValidationResult("Please provide the detail for: Due diligence processes");

            if (IncludesTraining == false && string.IsNullOrWhiteSpace(TrainingDetails))
                yield return new ValidationResult("Please provide the detail for: Staff training about slavery and human trafficking");

            if (IncludesGoals == false && string.IsNullOrWhiteSpace(GoalsDetails))
                yield return new ValidationResult("Please provide the detail for: Goals and key performance indicators (KPIs) to measure your progress over time, and the effectiveness of your actions");
        }

        public override bool IsComplete()
        {
            return IncludesStructure.HasValue
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
                && ((IncludesGoals == true) || !string.IsNullOrWhiteSpace(GoalsDetails));


        }
    }
}
