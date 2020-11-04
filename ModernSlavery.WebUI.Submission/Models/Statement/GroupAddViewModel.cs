using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class GroupAddViewModelMapperProfile : Profile
    {
        public GroupAddViewModelMapperProfile()
        {
            CreateMap<StatementModel, GroupAddViewModel>()
                .ForMember(d=>d.SearchKeywords, opt=>opt.Ignore())
                .ForMember(d=>d.NewOrganisationName, opt=>opt.Ignore())
                .ForMember(d => d.StatementOrganisations, opt => opt.MapFrom(s => s.StatementOrganisations));

            CreateMap<GroupAddViewModel, StatementModel>(MemberList.None)
                .ForMember(s => s.StatementOrganisations, opt => opt.MapFrom(d => d.StatementOrganisations));
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
        public string NewOrganisationName { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();
            return validationResults;
        }
    }
}
