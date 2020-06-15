using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public partial class StatementMetadata
    {
        public long StatementMetadataId { get; set; }

        public long OrganisationId { get; set; }

        public ReturnStatuses Status { get; set; }

        // Date the status last changed
        public DateTime StatusDate { get; set; }

        // Earliest date that the submission can be started
        public DateTime ReportingStartDate { get; set; }

        // Latest date that the submission can be started
        public DateTime ReportingEndDate { get; set; }

        public virtual Organisation Organisation { get; set; }
    }
}
