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
            CreateMap<StatementModel,PoliciesPageViewModel>();
            CreateMap<PoliciesPageViewModel, StatementModel>(MemberList.Source);
        }
    }

    public class PoliciesPageViewModel : BaseViewModel
    {
        public IList<PolicyViewModel> Policies { get; set; }

        public string OtherPolicies { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //better way to identify this checkbox
            if (Policies.Single(x => x.Description == "Other").IsSelected && OtherPolicies.IsNullOrWhiteSpace())
                yield return new ValidationResult("Please provide detail on 'other'");
        }

        public class PolicyViewModel
        {
            public short Id { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}

