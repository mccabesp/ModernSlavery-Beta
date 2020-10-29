using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Binding;
using ModernSlavery.Core.Entities.StatementSummary;
using static ModernSlavery.Core.Entities.StatementSummary.IStatementSummary1;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class PoliciesPageViewModelMapperProfile : Profile
    {
        public PoliciesPageViewModelMapperProfile()
        {
            CreateMap<StatementSummary1, PoliciesPageViewModel>();

            CreateMap<PoliciesPageViewModel, StatementSummary1>(MemberList.Source)
                .ForMember(d => d.Policies, opt => opt.MapFrom(s=>s.Policies))
                .ForMember(d => d.OtherPolicies, opt => opt.MapFrom(s=>s.OtherPolicies))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class PoliciesPageViewModel : BaseViewModel
    {
        public override string PageTitle => "Do your organisation’s policies and codes include any of the following provisions in relation to modern slavery?";

        public List<PolicyTypes> Policies { get; set; } = new List<PolicyTypes>();

        [MaxLength(256)]//We need at least one validation annotation otherwise Validate wont execute
        public string OtherPolicies { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (Policies.Contains(PolicyTypes.Other) && string.IsNullOrWhiteSpace(OtherPolicies))
                validationResults.AddValidationError(3400, nameof(OtherPolicies));

            return validationResults;
        }

        public override bool IsComplete()
        {
            return Policies.Any()
                && (!Policies.Contains(PolicyTypes.Other) || !string.IsNullOrWhiteSpace(OtherPolicies));
        }
    }
}

