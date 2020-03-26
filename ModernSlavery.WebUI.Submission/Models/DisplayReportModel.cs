using System;
using ModernSlavery.BusinessDomain.Shared.Models;

namespace ModernSlavery.WebUI.Submission.Models
{

    [Serializable]
    public class DisplayReportModel
    {

        public long OrganisationId { get; set; }

        public string EncCurrentOrgId { get; set; }

        public ReportInfoModel Report { get; set; }

        public bool CanChangeScope { get; set; }

    }

}
