﻿using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class UrlEmailViewModelMapperProfile : Profile
    {
        public UrlEmailViewModelMapperProfile()
        {
            CreateMap<StatementModel, UrlEmailViewModel>();

            CreateMap<UrlEmailViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForMember(d => d.StatementUrl, opt => opt.MapFrom(s => s.StatementUrl))
                .ForMember(d => d.StatementEmail, opt => opt.MapFrom(s => s.StatementEmail));

        }
    }

    public class UrlEmailViewModel : BaseStatementViewModel
    {
        public override string PageTitle => "Provide a link to the modern slavery statement on your organisation's website";

        [AbsoluteUrl]
        [TextLength(1024)]
        public string StatementUrl { get; set; }

        [EmailAddress]
        [TextLength(254)]
        public string StatementEmail { get; set; }


        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (string.IsNullOrWhiteSpace(StatementUrl) && string.IsNullOrWhiteSpace(StatementEmail))
            {
                validationResults.AddValidationError(3102, nameof(StatementUrl));
                validationResults.AddValidationError(3102, nameof(StatementEmail));
            }

            if (!string.IsNullOrWhiteSpace(StatementUrl) && !string.IsNullOrWhiteSpace(StatementEmail))
            {
                validationResults.AddValidationError(3105, nameof(StatementUrl));
                validationResults.AddValidationError(3105, nameof(StatementEmail));
            }


            return validationResults;
        }

        public override Status GetStatus()
        {
            if (!string.IsNullOrWhiteSpace(StatementUrl) || !string.IsNullOrWhiteSpace(StatementEmail)) return Status.Complete;
            return Status.Incomplete;
        }

    }
}