using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.WebUI.Submission.Classes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Binding;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class PoliciesPageViewModelMapperProfile : Profile
    {
        public PoliciesPageViewModelMapperProfile()
        {
            CreateMap<StatementModel,PoliciesPageViewModel>();

            CreateMap<PoliciesPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForSourceMember(s => s.PolicyTypes, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.PageTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SubTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ReportingDeadlineYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.BackUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.CancelUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ContinueUrl, opt => opt.DoNotValidate());
        }
    }

    [DependencyModelBinder]
    public class PoliciesPageViewModel : BaseViewModel
    {
        [IgnoreMap]
        [Newtonsoft.Json.JsonIgnore]//This needs to be Newtonsoft.Json.JsonIgnore namespace not System.Text.Json.Serialization.JsonIgnore
        public PolicyTypeIndex PolicyTypes { get; }
        public PoliciesPageViewModel(PolicyTypeIndex policyTypes)
        {
            PolicyTypes = policyTypes;
        }

        public PoliciesPageViewModel()
        {

        }

        public override string PageTitle => "Policies";

        public List<short> Policies { get; set; } = new List<short>();

        [MaxLength(1024)]//We need at least one validation annotation otherwise Validate wont execute
        public string OtherPolicies { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            //better way to identify this checkbox
            var otherId = PolicyTypes.Single(x => x.Description.Equals("Other")).Id;

            if (Policies.Contains(otherId) && string.IsNullOrWhiteSpace(OtherPolicies))
                validationResults.AddValidationError(3400, nameof(OtherPolicies));

            return validationResults;
        }

        public override bool IsComplete()
        {
            var other = PolicyTypes.Single(x => x.Description.Equals("Other"));

            return Policies.Any()
                && !Policies.Any(p=>p==other.Id && string.IsNullOrWhiteSpace(OtherPolicies));
        }
    }
}

