﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
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
                .ForMember(d => d.NewOrganisationName, opt => opt.Ignore())
                .ForMember(d => d.StatementOrganisations, opt => opt.MapFrom(s => s.StatementOrganisations));

            CreateMap<GroupAddViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForMember(s => s.StatementOrganisations, opt => opt.MapFrom(d => d.StatementOrganisations));
        }
    }

    public class GroupAddViewModel : GroupOrganisationsViewModel
    {
        public override string PageTitle => "Which organisations are included in your group statement?";


        [Required(AllowEmptyStrings = false)]
        [MaxLength(160)]
        [Text] 
        public string NewOrganisationName { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();
            return validationResults;
        }
    }
}
