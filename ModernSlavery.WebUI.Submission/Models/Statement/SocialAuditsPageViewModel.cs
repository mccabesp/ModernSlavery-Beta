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
    public class SocialAuditsPageViewModelMapperProfile : Profile
    {
        public SocialAuditsPageViewModelMapperProfile()
        {
            CreateMap<StatementSummary1, SocialAuditsPageViewModel>();

            CreateMap<SocialAuditsPageViewModel, StatementSummary1>(MemberList.Source)
                .ForMember(d => d.SocialAudits, opt => opt.MapFrom(s=>s.SocialAudits))
                .ForMember(d => d.OtherSocialAudits, opt => opt.MapFrom(s=>s.OtherSocialAudits))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class SocialAuditsPageViewModel : BaseViewModel
    {
        public override string PageTitle => "What type of social audits did you carry out?";

        public List<SocialAuditTypes> SocialAudits { get; set; } = new List<SocialAuditTypes>();

        [MaxLength(256)]
        public string OtherSocialAudits { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (SocialAudits.Contains(SocialAuditTypes.Other) && string.IsNullOrWhiteSpace(OtherSocialAudits))
                validationResults.AddValidationError(4100, nameof(OtherSocialAudits));

            return validationResults;
        }

        public override bool IsComplete()
        {
            return SocialAudits.Any()
                && (!SocialAudits.Contains(SocialAuditTypes.Other) || !string.IsNullOrWhiteSpace(OtherSocialAudits));
        }
    }
}
