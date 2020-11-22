using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using static ModernSlavery.Core.Entities.StatementSummary.IStatementSummary1;
using ModernSlavery.Core.Entities.StatementSummary;
using ModernSlavery.BusinessDomain.Shared.Models;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class IndicatorViewModelMapperProfile : Profile
    {
        public IndicatorViewModelMapperProfile()
        {
            CreateMap<StatementModel, IndicatorsViewModel>()
                .ForMember(d => d.Indicators, opt => opt.MapFrom(s => s.Summary.Indicators));
                //.ForMember(d => d.OtherIndicators, opt => opt.MapFrom(s => s.Summary.OtherIndicators));

            CreateMap<IndicatorsViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForPath(d => d.Summary.Indicators, opt => opt.MapFrom(s => s.Indicators));
                //.ForPath(d => d.Summary.OtherIndicators, opt => opt.MapFrom(s=>s.OtherIndicators));
        }
    }

    public class IndicatorsViewModel : BaseStatementViewModel
    {
        public override string PageTitle => "Does your statement refer to finding any ILO indicators of forced labour?";

        public List<IndicatorTypes> Indicators { get; set; } = new List<IndicatorTypes>();

        //[MaxLength(256)]
        //public string OtherIndicators { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            //if (Indicators.Contains(IndicatorTypes.Other) && string.IsNullOrWhiteSpace(OtherIndicators))
            //    validationResults.AddValidationError(4400, nameof(OtherIndicators));

            if (Indicators.Contains(IndicatorTypes.None) && Indicators.Count() > 1)
                validationResults.AddValidationError(4401);

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (Indicators.Any())
            {
                //if (Indicators.Contains(IndicatorTypes.Other) && string.IsNullOrWhiteSpace(OtherIndicators)) return Status.InProgress;
                return Status.Complete;
            }

            return Status.Incomplete;
        }
    }
}
