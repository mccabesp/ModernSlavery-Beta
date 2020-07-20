using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models
{
    public class RisksPageViewModel
    {
        public int Year { get; set; }
        public string OrganisationIdentifier { get; set; }

        public List<RiskViewModel> RelevantRisks { get; set; }
        public string OtherRelevantRisks;

        public List<RiskViewModel> HighRisks { get; set; }
        public string OtherHighRisks;

        public List<RiskViewModel> LocationRisks { get; set; }

        public class RiskViewModel
        {
            public short Id { get; set; }
            public short? ParentId { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}
