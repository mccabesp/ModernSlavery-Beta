using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class GroupAddViewModelMapperProfile : Profile
    {
        public GroupAddViewModelMapperProfile()
        {
            CreateMap<GroupAddViewModel, StatementModel>(MemberList.Source)
                .IncludeBase<GroupOrganisationsViewModel, StatementModel>()
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<StatementModel, GroupAddViewModel>()
                .IncludeBase<StatementModel, GroupOrganisationsViewModel>()
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class GroupAddViewModel : GroupOrganisationsViewModel
    {  
        public override string PageTitle => "Which organisations are included in your group statement?";

        [Required(AllowEmptyStrings = false)]
        [StringLength(100,ErrorMessage = "You must enter an organisations name or company number between 3 and 100 characters in length",MinimumLength = 3)]
        public string SearchKeywords { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(100)]
        public string OrganisationName { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();
            return validationResults;
        }
    }
}
