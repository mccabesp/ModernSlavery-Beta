using System;
using ModernSlavery.BusinessLogic.Models.Organisation;

namespace ModernSlavery.WebUI.Models.Organisation
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
