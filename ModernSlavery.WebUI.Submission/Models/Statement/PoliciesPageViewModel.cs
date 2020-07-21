using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class PoliciesPageViewModelMapperProfile : Profile
    {
        public PoliciesPageViewModelMapperProfile()
        {
            CreateMap<PoliciesPageViewModel, StatementModel>();
        }
    }

    public class PoliciesPageViewModel : BaseViewModel
    {
        public IList<PolicyViewModel> Policies { get; set; }

        public string OtherPolicies { get; set; }

        public class PolicyViewModel
        {
            public short Id { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}
