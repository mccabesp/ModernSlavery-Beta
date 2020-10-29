using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using static ModernSlavery.Core.Entities.StatementSummary.IStatementSummary1;
using ModernSlavery.Core.Entities.StatementSummary;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class RemediationPageViewModelMapperProfile : Profile
    {
        public RemediationPageViewModelMapperProfile()
        {
            CreateMap<StatementSummary1, RemediationPageViewModel>();

            CreateMap<RemediationPageViewModel, StatementSummary1>(MemberList.Source)
                .ForMember(d => d.Remediations, opt => opt.MapFrom(s=>s.Remediations))
                .ForMember(d => d.OtherRemediations, opt => opt.MapFrom(s=>s.OtherRemediations))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class RemediationPageViewModel : BaseViewModel
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

        public override bool IsComplete()
        {
            return Remediations.Any()
                && (!Remediations.Contains(RemediationTypes.Other) || !string.IsNullOrWhiteSpace(OtherRemediations));
        }
    }
}
