using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class GroupStatusViewModelMapperProfile : Profile
    {
        public GroupStatusViewModelMapperProfile()
        {
            CreateMap<GroupStatusViewModel, StatementModel>(MemberList.Source)
                .IncludeBase<GroupOrganisationsViewModel, StatementModel>()
                .ForMember(d => d.StatementOrganisations, opt => opt.Ignore());

            CreateMap<StatementModel, GroupStatusViewModel>()
                .IncludeBase<StatementModel, GroupOrganisationsViewModel>()
                .ForMember(d => d.StatementOrganisations, opt => opt.Ignore());
        }
    }

    public class GroupStatusViewModel : GroupOrganisationsViewModel
    {
        public override string PageTitle => "Who is your statement for?";

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();
            if (GroupSubmission==null)validationResults.AddValidationError(3900, nameof(GroupSubmission));
            return validationResults;
        }
    }
}
