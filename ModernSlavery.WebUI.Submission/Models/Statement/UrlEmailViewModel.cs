﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ModernSlavery.WebUI.Submission.Models.Statement.GroupOrganisationsViewModel;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class UrlEmailViewModelMapperProfile : Profile
    {
        public UrlEmailViewModelMapperProfile()
        {
            CreateMap<StatementModel, UrlEmailViewModel>();

            CreateMap<UrlEmailViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.StatementUrl, opt => opt.MapFrom(s=>s.StatementUrl))
                .ForMember(d => d.StatementEmail, opt => opt.MapFrom(s=>s.StatementEmail))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class UrlEmailViewModel : BaseViewModel
    {
        public override string PageTitle => "Provide a link to the modern slavery statement on your organisation's website";

        [Url]
        [MaxLength(256)]
        public string StatementUrl { get; set; }

        [EmailAddress]
        [MaxLength(256)]
        public string StatementEmail { get; set; }


        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            //TODO: Any validation required here?

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (!string.IsNullOrWhiteSpace(StatementUrl) || !string.IsNullOrWhiteSpace(StatementUrl)) return Status.Complete;
            return Status.Incomplete;
        }

    }
}