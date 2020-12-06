using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Entities.StatementSummary.V1;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using static ModernSlavery.Core.Entities.StatementSummary.V1.StatementSummary;
using static ModernSlavery.Core.Entities.StatementSummary.V1.StatementSummary.StatementRisk;
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
                .ForMember(d => d.Description, opt => opt.MapFrom((s, d) => s.Summary.Risks[d.Index].Description))
                .ForMember(d => d.Targets, opt => opt.MapFrom((s, d) => new List<RiskTargetTypes>(s.Summary.Risks[d.Index].Targets)))
                .ForMember(d => d.OtherTargets, opt => opt.MapFrom((s, d) => s.Summary.Risks[d.Index].OtherTargets))
                .ForMember(d => d.LikelySource, opt => opt.MapFrom((s, d) => s.Summary.Risks[d.Index].LikelySource))
                .ForMember(d => d.OtherLikelySource, opt => opt.MapFrom((s, d) => s.Summary.Risks[d.Index].OtherLikelySource))
                .ForMember(d => d.ActionsOrPlans, opt => opt.MapFrom((s, d) => s.Summary.Risks[d.Index].ActionsOrPlans))
                .ForMember(d => d.SupplyChainTiers, opt => opt.MapFrom((s, d) => new List<SupplyChainTierTypes>(s.Summary.Risks[d.Index].SupplyChainTiers)))
                .ForMember(d => d.CountryReferences, opt => opt.MapFrom((s, d) => new List<string>(s.Summary.Risks[d.Index].Countries.Select(c => c.FullReference))));

            CreateMap<HighRiskViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ConvertUsing<RiskConverter>();
        }

        public class RiskConverter : ITypeConverter<HighRiskViewModel, StatementModel>
        {
            readonly IGovUkCountryProvider CountryProvider;

            public RiskConverter(IGovUkCountryProvider countryProvider)
            {
                CountryProvider = countryProvider;
            }

            public StatementModel Convert(HighRiskViewModel source, StatementModel destination, ResolutionContext context)
            {
                destination.Summary.Risks[source.Index].Targets = new SortedSet<RiskTargetTypes>(source.Targets);
                destination.Summary.Risks[source.Index].OtherTargets = source.Targets.Contains(RiskTargetTypes.Other) ? source.OtherTargets : null;
                destination.Summary.Risks[source.Index].LikelySource = source.LikelySource;
                destination.Summary.Risks[source.Index].OtherLikelySource = source.LikelySource == RiskSourceTypes.Other ? source.OtherLikelySource : null;
                destination.Summary.Risks[source.Index].ActionsOrPlans = source.ActionsOrPlans;
                destination.Summary.Risks[source.Index].SupplyChainTiers = source.LikelySource == RiskSourceTypes.SupplyChains ? source.SupplyChainTiers.ToList() : null;
                var countries = source.CountryReferences.Select(r => CountryProvider.FindByReference(r)).ToList();
                destination.Summary.Risks[source.Index].Countries = new SortedSet<GovUkCountry>(countries);

                return destination;
            }
        }
    }

    public class HighRiskViewModel : BaseStatementViewModel
    {
        public HighRiskViewModel()
        {
        }

        public HighRiskViewModel(int index)
        {
            Index = index;
        }

        public override string PageTitle => "About this risk";

        public int Index { get; set; } = -1;

        public int TotalRisks { get; set; }

        public string Description { get; set; }

        public List<RiskTargetTypes> Targets { get; set; } = new List<RiskTargetTypes>();

        public string OtherTargets { get; set; }

        public RiskSourceTypes LikelySource { get; set; }

        public string OtherLikelySource { get; set; }

        public List<SupplyChainTierTypes> SupplyChainTiers { get; set; } = new List<SupplyChainTierTypes>();

        [MaxLength(200)]
        public string ActionsOrPlans { get; set; }

        [IgnoreMap]
        public string SelectedCountry { get; set; }

        public List<string> CountryReferences { get; set; } = new List<string>();

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (LikelySource == RiskSourceTypes.Other && string.IsNullOrWhiteSpace(OtherLikelySource))
                validationResults.AddValidationError(4700, nameof(OtherLikelySource));

            if (Targets.Contains(RiskTargetTypes.Other) && string.IsNullOrWhiteSpace(OtherTargets))
                validationResults.AddValidationError(4700, nameof(OtherTargets));

            if (LikelySource == RiskSourceTypes.SupplyChains)
            {
                if (!SupplyChainTiers.Any())
                    validationResults.AddValidationError(4700, nameof(SupplyChainTiers));

                else if (SupplyChainTiers.Contains(SupplyChainTierTypes.None) && SupplyChainTiers.Count > 1)
                    validationResults.AddValidationError(4701, nameof(SupplyChainTiers));
            }

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (LikelySource == RiskSourceTypes.Other && string.IsNullOrWhiteSpace(OtherLikelySource)) return Status.InProgress;
            if (LikelySource == RiskSourceTypes.SupplyChains && !SupplyChainTiers.Any()) return Status.InProgress;
            if (Targets.Contains(RiskTargetTypes.Other) && string.IsNullOrWhiteSpace(OtherTargets)) return Status.InProgress;
            if (LikelySource != RiskSourceTypes.Unknown && Targets.Any()) return Status.Complete;

            return Status.Incomplete;
        }
    }
}
