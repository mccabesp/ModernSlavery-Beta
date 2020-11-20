using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Entities.StatementSummary;
using ModernSlavery.Core.Extensions;
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
                .ForMember(d => d.Description, opt => opt.MapFrom((s, d) => s.Summary.Risks[d.Index].Description))
                .ForMember(d => d.Targets, opt => opt.MapFrom((s, d) => new List<RiskTargetTypes>(s.Summary.Risks[d.Index].Targets)))
                .ForMember(d => d.OtherTargets, opt => opt.MapFrom((s, d) => s.Summary.Risks[d.Index].OtherTargets))
                .ForMember(d => d.LikelySource, opt => opt.MapFrom((s, d) => s.Summary.Risks[d.Index].LikelySource))
                .ForMember(d => d.OtherLikelySource, opt => opt.MapFrom((s, d) => s.Summary.Risks[d.Index].OtherLikelySource))
                .ForMember(d => d.ActionsOrPlans, opt => opt.MapFrom((s, d) => s.Summary.Risks[d.Index].ActionsOrPlans))
                .ForMember(d => d.SupplyChainTiers, opt => opt.MapFrom((s, d) => new List<SupplyChainTierTypes>(s.Summary.Risks[d.Index].SupplyChainTiers)))
                .ForMember(d => d.Countries, opt => opt.MapFrom((s, d) => new List<CountryTypes>(s.Summary.Risks[d.Index].Countries)));

            CreateMap<HighRiskViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .AfterMap((s, d) =>
                {
                    d.Summary.Risks[s.Index].Targets = new SortedSet<RiskTargetTypes>(s.Targets);
                    d.Summary.Risks[s.Index].OtherTargets = s.Targets.Contains(RiskTargetTypes.Other) ? s.OtherTargets : null;
                    d.Summary.Risks[s.Index].LikelySource = s.LikelySource;
                    d.Summary.Risks[s.Index].OtherLikelySource = s.LikelySource == RiskSourceTypes.Other ? s.OtherLikelySource : null;
                    d.Summary.Risks[s.Index].ActionsOrPlans = s.ActionsOrPlans;
                    d.Summary.Risks[s.Index].SupplyChainTiers = s.LikelySource == RiskSourceTypes.SupplyChains ? s.SupplyChainTiers.ToList() : null;
                    d.Summary.Risks[s.Index].Countries = new SortedSet<CountryTypes>(s.Countries);
                });
        }
    }

    public class HighRiskViewModel : BaseViewModel
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

        public List<CountryTypes> Countries { get; set; } = new List<CountryTypes>();

        public bool TryAddSelectedCountry()
        {
            if (string.IsNullOrWhiteSpace(SelectedCountry))
                return false;

            var countries = Enums.GetValuesExcept<CountryTypes>(CountryTypes.Unknown)
                .Select(c => new { value = c, description = c.GetEnumDescription() });

            var options = countries.Where(c => c.description.Equals(SelectedCountry, StringComparison.OrdinalIgnoreCase));

            if (options.Count() != 1)
                return false;

            var option = options.Single();

            if (Countries.Contains(option.value))
                return false;

            Countries.Add(option.value);
            SelectedCountry = null;
            return true;
        }

        public bool TryRemoveCountry(CountryTypes countryType)
        {
            if (Countries.Contains(countryType))
            {
                Countries.Remove(countryType);
                return true;
            }

            return false;
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            //TODO: Double check error numbers are all correct - I just copied and pasted for now
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
