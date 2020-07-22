using ModernSlavery.WebUI.GDSDesignSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Parsers;
using ModernSlavery.Core.Extensions;
using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

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
                .ForSourceMember(s => s.BackUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.CancelUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ContinueUrl, opt => opt.DoNotValidate());
        }
    }
    public class CompliancePageViewModel:BaseViewModel
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

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IncludesStructure == false && StructureDetails.IsNullOrWhiteSpace())
                yield return new ValidationResult("Please provide the detail for: Your organisatio's structure, business and supply chains");

            if (IncludesPolicies == false && PolicyDetails.IsNullOrWhiteSpace())
                yield return new ValidationResult("Please provide the detail for: Policies");

            if (IncludesRisks == false && RisksDetails.IsNullOrWhiteSpace())
                yield return new ValidationResult("Please provide the detail for: Risk assessment and management");

            if (IncludesDueDiligence == false && DueDiligenceDetails.IsNullOrWhiteSpace())
                yield return new ValidationResult("Please provide the detail for: Due diligence processes");

            if (IncludesTraining == false && TrainingDetails.IsNullOrWhiteSpace())
                yield return new ValidationResult("Please provide the detail for: Staff training about slavery and human trafficking");

            if (IncludesGoals == false && GoalsDetails.IsNullOrWhiteSpace())
                yield return new ValidationResult("Please provide the detail for: Goals and key performance indicators (KPIs) to measure your progress over time, and the effectiveness of your actions");
        }
    }
}
