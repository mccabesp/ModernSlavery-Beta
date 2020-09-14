using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class CancelPageViewModelMapperProfile : Profile
    {
        public CancelPageViewModelMapperProfile()
        {
            CreateMap<StatementModel, CancelPageViewModel>();

            CreateMap<CancelPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(s => s.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForSourceMember(s => s.Modifications, opt => opt.DoNotValidate())
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
        public IList<AutoMap.Diff> Modifications { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }

        public bool HasChanged()
        {
            return Modifications!=null && Modifications.Any();
        }
    }
}
