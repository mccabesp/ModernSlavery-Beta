using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models
{
    public class OrganisationPageViewModel
    {
        public int Year { get; set; }
        public string OrganisationIdentifier { get; set; }

        public IList<SectorViewModel> AllSectors { get; set; }
        public IList<SectorViewModel> Sectors { get; set; }

        public Presenters.LastFinancialYearBudget Turnover { get; set; }

        public class SectorViewModel
        {
            public short Id { get; set; }
            public string Description { get; set; }
        }
    }
}
