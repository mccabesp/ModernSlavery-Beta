using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Entities.StatementSummary;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ModernSlavery.Core.Entities.StatementSummary.IStatementSummary1;
using static ModernSlavery.Core.Entities.StatementSummary.IStatementSummary1.StatementRisk;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class RiskDetailsPageViewModelMapperProfile : Profile
    {
        public RiskDetailsPageViewModelMapperProfile()
        {
            CreateMap<StatementRisk, RiskDetailsPageViewModel>();

            CreateMap<RiskDetailsPageViewModel, StatementRisk>(MemberList.Source)
                .ForMember(d=>d.LikelySource,opt=>opt.MapFrom(s=>s.LikelySource))
                .ForMember(d=>d.OtherLikelySource,opt=>opt.MapFrom(s=>s.OtherLikelySource))
                .ForMember(d=>d.Targets,opt=>opt.MapFrom(s=>s.Targets))
                .ForMember(d=>d.OtherTargets,opt=>opt.MapFrom(s=>s.OtherTargets))
                .ForMember(d=>d.Countries, opt=>opt.MapFrom(s=>s.Countries))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class RiskDetailsPageViewModel : BaseViewModel
    {
        public override string PageTitle => "About this risk";

        [BindNever]
        public string Description { get; set; }

        public RiskSourceTypes? LikelySource { get; set; }

        public string OtherLikelySource { get; set; }

        public List<RiskTargetTypes> Targets { get; set; } = new List<RiskTargetTypes>();
        public string OtherTargets { get; set; }

        public List<CountryTypes> Countries { get; set; } = new List<CountryTypes>();

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (LikelySource==RiskSourceTypes.Other && string.IsNullOrWhiteSpace(OtherLikelySource))
                validationResults.AddValidationError(4700, nameof(OtherLikelySource));

            if (Targets.Contains(RiskTargetTypes.Other) && string.IsNullOrWhiteSpace(OtherTargets))
                validationResults.AddValidationError(4700, nameof(OtherTargets));

            return validationResults;
        }

        public override bool IsComplete()
        {
            return (LikelySource == null || (LikelySource== RiskSourceTypes.Other && !string.IsNullOrWhiteSpace(OtherTargets)))
                && (Targets == null || Targets.Count == 0 || (Targets.Contains(RiskTargetTypes.Other) && !string.IsNullOrWhiteSpace(OtherTargets)));
        }
    }
}
