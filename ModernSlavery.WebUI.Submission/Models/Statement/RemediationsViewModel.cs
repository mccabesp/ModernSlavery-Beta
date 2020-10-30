﻿using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using static ModernSlavery.Core.Entities.StatementSummary.IStatementSummary1;
using ModernSlavery.Core.Entities.StatementSummary;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class RemediationsViewModelMapperProfile : Profile
    {
        public RemediationsViewModelMapperProfile()
        {
            CreateMap<StatementSummary1, RemediationsViewModel>();

            CreateMap<RemediationsViewModel, StatementSummary1>(MemberList.Source)
                .ForMember(d => d.Remediations, opt => opt.MapFrom(s=>s.Remediations))
                .ForMember(d => d.OtherRemediations, opt => opt.MapFrom(s=>s.OtherRemediations))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class RemediationsViewModel : BaseViewModel
    {
        public override string PageTitle => "What action did you take in response?";

        public List<RemediationTypes> Remediations { get; set; } = new List<RemediationTypes>();

        [MaxLength(256)]
        public string OtherRemediations { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (Remediations.Contains(RemediationTypes.Other) && string.IsNullOrWhiteSpace(OtherRemediations))
                validationResults.AddValidationError(4500, nameof(OtherRemediations));

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (Remediations.Any())
            {
                if (Remediations.Contains(RemediationTypes.Other) && string.IsNullOrWhiteSpace(OtherRemediations)) return Status.InProgress;
                return Status.Complete;
            }

            return Status.Incomplete;
        }
    }
}