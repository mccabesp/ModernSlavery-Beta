using System;
using ModernSlavery.Core.SharedKernel;

namespace ModernSlavery.BusinessDomain.Shared.Models
{
    [Serializable]
    public class LateSubmissionsFileModel
    {
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public SectorTypes OrganisationSectorType { get; set; }
        public long ReportId { get; set; }
        public DateTime ReportSnapshotDate { get; set; }
        public string ReportLateReason { get; set; }
        public DateTime ReportSubmittedDate { get; set; }
        public DateTime ReportModifiedDate { get; set; }
        public string ReportModifiedFields { get; set; }
        public string ReportPersonResonsible { get; set; }
        public bool ReportEHRCResponse { get; set; }
    }
}