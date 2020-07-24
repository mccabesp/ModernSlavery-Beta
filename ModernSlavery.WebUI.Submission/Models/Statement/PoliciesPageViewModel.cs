﻿using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Submission.Classes;
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
        public PoliciesPageViewModel(PolicyTypeIndex policyTypes)
        {
            PolicyTypes = policyTypes;
        }
        public override string PageTitle => "Policies";

        public PolicyTypeIndex PolicyTypes { get; set; }

        public IList<short> Policies { get; set; }

        public string OtherPolicies { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //better way to identify this checkbox
            var otherId = PolicyTypes.Single(x => x.Description.Equals("other")).Id;
            if (Policies.Contains(otherId) && string.IsNullOrWhiteSpace(OtherPolicies))
                yield return new ValidationResult("Please provide detail on 'other'");
        }

        public override bool IsComplete()
        {
            return base.IsComplete();
        }
    }
}

