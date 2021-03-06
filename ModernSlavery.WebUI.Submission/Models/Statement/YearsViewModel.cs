﻿using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ModernSlavery.Core.Entities.Statement;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class YearsViewModelMapperProfile : Profile
    {
        public YearsViewModelMapperProfile()
        {
            CreateMap<StatementModel, YearsViewModel>();

            CreateMap<YearsViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForMember(d => d.StatementYears, opt => opt.MapFrom(s => s.StatementYears));
        }
    }

    public class YearsViewModel : BaseStatementViewModel
    {
        public override string PageTitle => "How many years has your organisation been producing modern slavery statements?";

        public StatementYearRanges? StatementYears { get; set; }

        public override Status GetStatus()
        {
            if (StatementYears != null && StatementYears != StatementYearRanges.NotProvided) return Status.Complete;

            return Status.Incomplete;
        }
    }
}
