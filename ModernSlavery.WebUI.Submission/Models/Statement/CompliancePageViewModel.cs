﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.WebUI.Shared.Classes.Extensions;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class CompliancePageViewModelMapperProfile : Profile
    {
        public CompliancePageViewModelMapperProfile()
        {
            CreateMap<StatementModel, CompliancePageViewModel>();

            CreateMap<CompliancePageViewModel, StatementModel>(MemberList.Source)
                .ForMember(s => s.OrganisationId, opt => opt.Ignore())
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
