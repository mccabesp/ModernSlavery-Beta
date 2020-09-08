using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.BusinessDomain.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class GroupSearchViewModelMapperProfile : Profile
    {
        public GroupSearchViewModelMapperProfile()
        {
            CreateMap<GroupSearchViewModel, StatementModel>(MemberList.Source)
                .IncludeBase<GroupOrganisationsViewModel, StatementModel>()
                .ForMember(d => d.GroupSubmission, opt => opt.Ignore())
                .ForSourceMember(d => d.GroupReviewUrl, opt => opt.DoNotValidate());

            CreateMap<StatementModel, GroupSearchViewModel>()
                .IncludeBase<StatementModel, GroupOrganisationsViewModel>()
                .ForMember(d => d.GroupSubmission, opt => opt.Ignore());
        }
    }

    public class GroupSearchViewModel : GroupOrganisationsViewModel
    {
        public override string PageTitle => "Who is your statement for?";
        [IgnoreMap]
        [BindNever]
        public string GroupReviewUrl { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            return validationResults;
        }
    }
}
