using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class CancelPageViewModelMapperProfile : Profile
    {
        public CancelPageViewModelMapperProfile()
        {
            CreateMap<StatementModel, CancelPageViewModel>();

            CreateMap<CancelPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForSourceMember(s => s.ErrorCount, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.PageTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SubTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ReportingDeadlineYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.BackUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.CancelUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ContinueUrl, opt => opt.DoNotValidate());
        }
    }

    public class CancelPageViewModel : BaseViewModel
    {
        public override string PageTitle => "Cancel Statement";

        [IgnoreMap]
        public int ErrorCount { get; set; }
        [IgnoreMap]
        public string Modifications { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }

        public bool HasChanged()
        {
            return !string.IsNullOrWhiteSpace(Modifications);
        }
    }
}
