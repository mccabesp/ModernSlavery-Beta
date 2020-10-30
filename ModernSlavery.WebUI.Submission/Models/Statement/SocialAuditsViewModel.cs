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
    public class SocialAuditsViewModelMapperProfile : Profile
    {
        public SocialAuditsViewModelMapperProfile()
        {
            CreateMap<StatementSummary1, SocialAuditsViewModel>();

            CreateMap<SocialAuditsViewModel, StatementSummary1>(MemberList.Source)
                .ForMember(d => d.SocialAudits, opt => opt.MapFrom(s=>s.SocialAudits))
                .ForMember(d => d.OtherSocialAudits, opt => opt.MapFrom(s=>s.OtherSocialAudits))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class SocialAuditsViewModel : BaseViewModel
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

        public override Status GetStatus()
        {
            if (SocialAudits.Any())
            {
                if (SocialAudits.Contains(SocialAuditTypes.Other) && string.IsNullOrWhiteSpace(OtherSocialAudits)) return Status.InProgress;
                return Status.Complete;
            }

            return Status.Incomplete;
        }
    }
}
