using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.BusinessDomain.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class GroupReviewViewModelMapperProfile : Profile
    {
        public GroupReviewViewModelMapperProfile()
        {
            CreateMap<GroupReviewViewModel, StatementModel>(MemberList.Source)
                .IncludeBase<GroupOrganisationsViewModel, StatementModel>()
                .ForMember(d => d.GroupSubmission, opt => opt.Ignore())
                .ForSourceMember(d => d.GroupStatusUrl, opt => opt.DoNotValidate());

            CreateMap<StatementModel, GroupReviewViewModel>()
                .IncludeBase<StatementModel, GroupOrganisationsViewModel>()
                .ForMember(d => d.GroupSubmission, opt => opt.Ignore());
        }
    }

    public class GroupReviewViewModel: GroupOrganisationsViewModel
    {
        public override string PageTitle => "Review the organisations in your group statement";
        [IgnoreMap]
        [BindNever]
        public string GroupStatusUrl { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            return validationResults;
        }
    }
}
