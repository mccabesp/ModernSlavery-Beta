using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.Core.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class PoliciesPageViewModelMapperProfile : Profile
    {
        public PoliciesPageViewModelMapperProfile()
        {
            CreateMap<StatementModel.PolicyModel, PoliciesPageViewModel.PolicyViewModel>().ReverseMap();

            CreateMap<StatementModel,PoliciesPageViewModel>()
                .ForMember(s => s.BackUrl, opt => opt.Ignore())
                .ForMember(s => s.CancelUrl, opt => opt.Ignore())
                .ForMember(s => s.ContinueUrl, opt => opt.Ignore());

            CreateMap<PoliciesPageViewModel, StatementModel>(MemberList.Source)
                .ForSourceMember(s => s.PageTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SubTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ReportingDeadlineYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.BackUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.CancelUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ContinueUrl, opt => opt.DoNotValidate());
        }
    }

    public class PoliciesPageViewModel : BaseViewModel
    {
        #region Types
        public class PolicyViewModel
        {
            public short Id { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
        }
        #endregion

        public override string PageTitle => "Policies";

        public IList<PolicyViewModel> Policies { get; set; }

        public string OtherPolicies { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //better way to identify this checkbox
            if (Policies.Single(x => x.Description == "Other").IsSelected && OtherPolicies.IsNullOrWhiteSpace())
                yield return new ValidationResult("Please provide detail on 'other'");
        }

        public override bool IsComplete()
        {
            return base.IsComplete();
        }
    }
}

