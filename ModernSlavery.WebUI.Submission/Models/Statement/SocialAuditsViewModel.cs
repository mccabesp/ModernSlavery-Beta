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
    public class SocialAuditsViewModelMapperProfile : Profile
    {
        public SocialAuditsViewModelMapperProfile()
        {
            CreateMap<StatementModel, SocialAuditsViewModel>()
                .ForMember(d => d.SocialAudits, opt => opt.MapFrom(s => s.Summary.SocialAudits));
                //.ForMember(d => d.OtherSocialAudits, opt => opt.MapFrom(s => s.Summary.OtherSocialAudits));

            CreateMap<SocialAuditsViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForPath(d => d.Summary.SocialAudits, opt => opt.MapFrom(s => s.SocialAudits));
                //.ForPath(d => d.Summary.OtherSocialAudits, opt => opt.MapFrom(s => s.OtherSocialAudits));
        }
    }

    public class SocialAuditsViewModel : BaseViewModel
    {
        public override string PageTitle => "Did you carry out any social audits? \n If so, what type?";

        public List<SocialAuditTypes> SocialAudits { get; set; } = new List<SocialAuditTypes>();

        //[MaxLength(256)]
        //public string OtherSocialAudits { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            //if (SocialAudits.Contains(SocialAuditTypes.Other) && string.IsNullOrWhiteSpace(OtherSocialAudits))
            //    validationResults.AddValidationError(4100, nameof(OtherSocialAudits));

            if (SocialAudits.Contains(SocialAuditTypes.None) && SocialAudits.Count() > 1)
                validationResults.AddValidationError(4101, nameof(SocialAudits));

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
