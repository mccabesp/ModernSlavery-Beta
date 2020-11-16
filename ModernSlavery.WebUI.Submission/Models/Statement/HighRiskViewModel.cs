using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Entities.StatementSummary;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using static ModernSlavery.Core.Entities.StatementSummary.IStatementSummary1;
using static ModernSlavery.Core.Entities.StatementSummary.IStatementSummary1.StatementRisk;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class HighRiskViewModelMapperProfile : Profile
    {
        public HighRiskViewModelMapperProfile()
        {
            CreateMap<StatementModel, HighRiskViewModel>()
                .ForMember(d => d.Index, opt => opt.Ignore())
                .ForMember(d => d.TotalRisks, opt => opt.MapFrom(s => s.Summary.Risks.Count))
                .ForMember(d => d.Risk, opt => opt.MapFrom((s, d) => s.Summary.Risks.Count > d.Index ? s.Summary.Risks[d.Index] : new StatementRisk()));

            CreateMap<HighRiskViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .BeforeMap((s, d) => s.Risk.Description = d.Summary.Risks[s.Index].Description)
                .AfterMap((s, d) => d.Summary.Risks[s.Index] = s.Risk);
        }
    }

    public class HighRiskViewModel : BaseViewModel
    {
        public HighRiskViewModel(int index = 0)
        {
            Index = index;
        }

        public override string PageTitle => "About this risk";

        [BindNever]
        public int Index { get; } = -1;

        [BindNever]
        public int TotalRisks { get; set; }

        public StatementRisk Risk { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            //TODO: Double check error numbers are all correct - I just copied and pasted for now
            if (Risk.LikelySource == RiskSourceTypes.Other && string.IsNullOrWhiteSpace(Risk.OtherLikelySource))
                validationResults.AddValidationError(4700, nameof(Risk.OtherLikelySource));

            if (Risk.LikelySource == RiskSourceTypes.SupplyChains && !Risk.SupplyChainTiers.Any(sct=> sct!= SupplyChainTierTypes.Unknown))
                validationResults.AddValidationError(4700, nameof(Risk.LikelySource));

            //Clear SupplyChainTiers when LikelySource not SupplyChains
            if (Risk.LikelySource != RiskSourceTypes.SupplyChains)
                Risk.SupplyChainTiers.Clear();

            if (Risk.Targets.Contains(RiskTargetTypes.Other) && string.IsNullOrWhiteSpace(Risk.OtherTargets))
                validationResults.AddValidationError(4700, nameof(Risk.OtherTargets));

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (Risk.LikelySource == RiskSourceTypes.Other && string.IsNullOrWhiteSpace(Risk.OtherLikelySource)) return Status.InProgress;
            if (Risk.LikelySource == RiskSourceTypes.SupplyChains && !Risk.SupplyChainTiers.Any(sct => sct != SupplyChainTierTypes.Unknown)) return Status.InProgress;
            if (Risk.Targets.Contains(RiskTargetTypes.Other) && string.IsNullOrWhiteSpace(Risk.OtherTargets)) return Status.InProgress;
            if (Risk.LikelySource != RiskSourceTypes.Unknown && Risk.Targets.Any()) return Status.Complete;

            return Status.Incomplete;
        }
    }
}
