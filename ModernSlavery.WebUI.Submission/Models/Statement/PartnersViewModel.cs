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
    public class PartnersViewModelMapperProfile : Profile
    {
        public PartnersViewModelMapperProfile()
        {
            CreateMap<StatementModel, PartnersViewModel>()
                .ForMember(d => d.Partners, opt => opt.MapFrom(s => s.Summary.Partners));

            CreateMap<PartnersViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForPath(d => d.Summary.Partners, opt => opt.MapFrom(s => s.Partners));
        }
    }

    public class PartnersViewModel : BaseStatementViewModel
    {
        public override string PageTitle => "During the period of the statement, who did you engage with to help you monitor working conditions across your operations and supply chain?";

        public List<PartnerTypes> Partners { get; set; } = new List<PartnerTypes>();

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (Partners.Contains(PartnerTypes.None) && Partners.Count() > 1)
                validationResults.AddValidationError(3803);

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (Partners.Any())
            {
                return Status.Complete;
            }

            return Status.Incomplete;
        }

    }
}
