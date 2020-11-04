using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class GroupReviewViewModelMapperProfile : Profile
    {
        public GroupReviewViewModelMapperProfile()
        {
            CreateMap<StatementModel, GroupReviewViewModel>()
                .ForMember(s => s.StatementOrganisations, opt => opt.MapFrom(d => d.StatementOrganisations));

            CreateMap<GroupReviewViewModel, StatementModel>(MemberList.None)
                .ForMember(s => s.StatementOrganisations, opt => opt.MapFrom(d => d.StatementOrganisations));

        }
    }

    public class GroupReviewViewModel: GroupOrganisationsViewModel
    {
        public override string PageTitle => "Review the organisations in your group statement";

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            return validationResults;
        }
    }
}
