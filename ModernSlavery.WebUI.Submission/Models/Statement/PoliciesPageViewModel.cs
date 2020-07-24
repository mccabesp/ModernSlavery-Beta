using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.WebUI.Submission.Classes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class PoliciesPageViewModelMapperProfile : Profile
    {
        public PoliciesPageViewModelMapperProfile()
        {
            CreateMap<StatementModel,PoliciesPageViewModel>()
                .ForMember(s => s.BackUrl, opt => opt.Ignore())
                .ForMember(s => s.CancelUrl, opt => opt.Ignore())
                .ForMember(s => s.ContinueUrl, opt => opt.Ignore());

            CreateMap<PoliciesPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
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
        public override string PageTitle => "Policies";

        public IList<short> Policies { get; set; }

        public string OtherPolicies { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //Get the policy types
            var policyTypes = validationContext.GetService<PolicyTypeIndex>();

            //better way to identify this checkbox
            var otherId = policyTypes.Single(x => x.Description.Equals("Other")).Id;
            if (Policies.Contains(otherId) && string.IsNullOrWhiteSpace(OtherPolicies))
                yield return new ValidationResult("Please provide detail on 'other'");
        }

        public override bool IsComplete()
        {
            return Policies.Any(x => x.IsSelected)
                && !Policies.Single(x => x.Description == "Other").IsSelected || !OtherPolicies.IsNullOrWhiteSpace();
        }
    }
}

