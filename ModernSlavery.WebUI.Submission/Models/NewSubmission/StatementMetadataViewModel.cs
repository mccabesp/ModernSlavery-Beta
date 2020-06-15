using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModernSlavery.Core.Entities;
using System;

namespace ModernSlavery.WebUI.Submission.Presenters
{
    [Serializable]
    public class StatementMetadataViewModel
    {
        // DB layer Id
        public long StatementMetadataId { get; set; }

        // Presentation layer Id
        public string StatementMetadataIdentifier { get; set; }

        public ReturnStatuses Status { get; set; }

        // Date the status last changed
        public DateTime StatusDate { get; set; }

        // Earliest date that the submission can be started
        public DateTime ReportingStartDate { get; set; }

        // This should never go over the wire!
        public long OrganisationId { get; set; }

        public string OrganisationIdentifier { get; set; }

        public int Year { get; set; }
    }
}
