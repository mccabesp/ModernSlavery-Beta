﻿using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using static ModernSlavery.Core.Entities.StatementSummary.V1.StatementSummary;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class PoliciesViewModelMapperProfile : Profile
    {
        public PoliciesViewModelMapperProfile()
        {
            CreateMap<StatementModel, PoliciesViewModel>()
                .ForMember(d => d.Policies, opt => opt.MapFrom(s => s.Summary.Policies))
                .ForMember(d => d.OtherPolicies, opt => opt.MapFrom(s => s.Summary.OtherPolicies));

            CreateMap<PoliciesViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForPath(d => d.Summary.Policies, opt => opt.MapFrom(s => s.Policies))
                .ForPath(d => d.Summary.OtherPolicies, opt => opt.MapFrom(s => s.Policies.Contains(PolicyTypes.Other) ? s.OtherPolicies : null));
        }
    }

    public class PoliciesViewModel : BaseStatementViewModel
    {
        public override string PageTitle => "Do your organisation's policies include any of the following provisions in relation to modern slavery?";

        public List<PolicyTypes> Policies { get; set; } = new List<PolicyTypes>();

        [MaxLength(256)]//We need at least one validation annotation otherwise Validate wont execute
        [Text] 
        public string OtherPolicies { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (Policies.Contains(PolicyTypes.Other) && string.IsNullOrWhiteSpace(OtherPolicies))
                validationResults.AddValidationError(3600, nameof(OtherPolicies));

            if (Policies.Contains(PolicyTypes.None) && Policies.Count() > 1)
                validationResults.AddValidationError(3602);

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (Policies.Any())
            {
                if (Policies.Contains(PolicyTypes.Other) && string.IsNullOrWhiteSpace(OtherPolicies)) return Status.InProgress;
                return Status.Complete;
            }

            return Status.Incomplete;
        }
    }
}

