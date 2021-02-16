using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using static ModernSlavery.Core.Entities.StatementSummary.V1.StatementSummary;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class RemediationsViewModelMapperProfile : Profile
    {
        public RemediationsViewModelMapperProfile()
        {
            CreateMap<StatementModel, RemediationsViewModel>()
                .ForMember(d => d.Indicators, opt => opt.MapFrom(s => s.Summary.Indicators))
                .ForMember(d => d.Remediations, opt => opt.MapFrom(s => s.Summary.Remediations))
                .ForMember(d => d.OtherRemediations, opt => opt.MapFrom(s => s.Summary.OtherRemediations));

            CreateMap<RemediationsViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForPath(d => d.Summary.Indicators, opt => opt.Ignore())
                .ForPath(d => d.Summary.Remediations, opt => opt.MapFrom(s => s.Remediations))
                .ForPath(d => d.Summary.OtherRemediations, opt => opt.MapFrom(s => s.Remediations.Contains(RemediationTypes.Other) ? s.OtherRemediations : null));
        }
    }

    public class RemediationsViewModel : BaseStatementViewModel
    {
        public override string PageTitle => "Does your statement refer to any actions your organisation took after finding indicators of forced labour?";

        public List<RemediationTypes> Remediations { get; set; } = new List<RemediationTypes>();

        [MaxLength(256)]
        [Text]
        public string OtherRemediations { get; set; }

        public List<IndicatorTypes> Indicators { get; set; } = new List<IndicatorTypes>();

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (Remediations.Contains(RemediationTypes.Other) && string.IsNullOrWhiteSpace(OtherRemediations))
                validationResults.AddValidationError(4500, nameof(OtherRemediations));

            if (Remediations.Contains(RemediationTypes.None) && Remediations.Count() > 1)
                validationResults.AddValidationError(4501);

            return validationResults;
        }

        public override Status GetStatus()
        {
            // nothing entered, ensure this triggers incomplete
            if (!Indicators.Any() && !Remediations.Any())
                return Status.Incomplete;

            if (Indicators.Contains(IndicatorTypes.None))
                return Status.Complete;

            else if (Remediations.Any())
            {
                if (Remediations.Contains(RemediationTypes.Other) && string.IsNullOrWhiteSpace(OtherRemediations)) return Status.InProgress;
                return Status.Complete;
            }

            return Status.Incomplete;
        }
    }
}
