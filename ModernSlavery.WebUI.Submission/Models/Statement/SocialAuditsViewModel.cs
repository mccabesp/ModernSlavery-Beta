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
    public class SocialAuditsViewModelMapperProfile : Profile
    {
        public SocialAuditsViewModelMapperProfile()
        {
            CreateMap<StatementModel, SocialAuditsViewModel>()
                .ForMember(d => d.SocialAudits, opt => opt.MapFrom(s => s.Summary.SocialAudits));

            CreateMap<SocialAuditsViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForPath(d => d.Summary.SocialAudits, opt => opt.MapFrom(s => s.SocialAudits));
        }
    }

    public class SocialAuditsViewModel : BaseStatementViewModel
    {
        public override string PageTitle => "Did you use social audits to look for signs of forced labour?";

        public List<SocialAuditTypes> SocialAudits { get; set; } = new List<SocialAuditTypes>();

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (SocialAudits.Contains(SocialAuditTypes.None) && SocialAudits.Count() > 1)
                validationResults.AddValidationError(4101);

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (SocialAudits.Any())
            {
                //if (SocialAudits.Contains(SocialAuditTypes.Other) && string.IsNullOrWhiteSpace(OtherSocialAudits)) return Status.InProgress;
                return Status.Complete;
            }

            return Status.Incomplete;
        }
    }
}
