﻿using AutoMapper;
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
            CreateMap<StatementModel, GroupStatusViewModel>()
                .IncludeBase<StatementModel, GroupOrganisationsViewModel>();

            CreateMap<GroupStatusViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForMember(d => d.StatementOrganisations, opt => opt.Ignore())
                .ForMember(d => d.GroupSubmission, opt => opt.MapFrom(s => s.GroupSubmission));
        }
    }

    public class GroupStatusViewModel : GroupOrganisationsViewModel
    {
        public override string PageTitle => "Does your modern slavery statement cover a single organisation or a group of organisations?";

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();
            if (GroupSubmission == null) validationResults.AddValidationError(3900, nameof(GroupSubmission));

            return validationResults;
        }
    }
}
