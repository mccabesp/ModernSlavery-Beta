using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.BusinessDomain.Submission;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class SubmissionCompleteViewModelMapperProfile : Profile
    {
        public SubmissionCompleteViewModelMapperProfile()
        {
            CreateMap<StatementModel, SubmissionCompleteViewModel>()
                .ForMember(s => s.BackUrl, opt => opt.Ignore())
                .ForMember(s => s.CancelUrl, opt => opt.Ignore())
                .ForMember(s => s.ContinueUrl, opt => opt.Ignore());

            CreateMap<SubmissionCompleteViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForSourceMember(s => s.PageTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SubTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ReportingDeadlineYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.BackUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.CancelUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ContinueUrl, opt => opt.DoNotValidate());

        }
    }

    public class SubmissionCompleteViewModel : BaseViewModel
    {
        public override string PageTitle => "Submission Complete";

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return null;
        }
    }
}
