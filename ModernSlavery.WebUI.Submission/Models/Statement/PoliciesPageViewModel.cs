using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models
{
    public class PoliciesPageViewModel
    {
        public int Year { get; set; }
        public string OrganisationIdentifier { get; set; }

        public IList<PolicyViewModel> AllPolicies { get; set; }
        public IList<PolicyViewModel> Policies { get; set; }

        public string OtherPolicies { get; set; }

        public class PolicyViewModel
        {
            public short Id { get; set; }
            public string Description { get; set; }
        }
    }
}
