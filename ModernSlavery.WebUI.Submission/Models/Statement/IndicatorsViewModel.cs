using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using static ModernSlavery.Core.Entities.StatementSummary.V1.StatementSummary;
using ModernSlavery.BusinessDomain.Shared.Models;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class IndicatorViewModelMapperProfile : Profile
    {
        public IndicatorViewModelMapperProfile()
        {
            CreateMap<StatementModel, IndicatorsViewModel>()
                .ForMember(d => d.Indicators, opt => opt.MapFrom(s => s.Summary.Indicators));

            CreateMap<IndicatorsViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForPath(d => d.Summary.Indicators, opt => opt.MapFrom(s => s.Indicators))
                .BeforeMap((s, d) => {
                    if (!s.Indicators.Any(i=>i!= IndicatorTypes.None))
                    {
                        d.Summary.Remediations = null;
                        d.Summary.OtherRemediations = null;
                    }
                });
        }
    }

    public class IndicatorsViewModel : BaseStatementViewModel
    {
        public override string PageTitle => "Does your statement refer to finding any International Labour Organization (ILO) indicators of forced labour?";

        public List<IndicatorTypes> Indicators { get; set; } = new List<IndicatorTypes>();

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (Indicators.Contains(IndicatorTypes.None) && Indicators.Count() > 1)
                validationResults.AddValidationError(4401);

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (Indicators.Any())
            {
                return Status.Complete;
            }

            return Status.Incomplete;
        }
    }
}
