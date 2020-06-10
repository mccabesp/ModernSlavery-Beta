using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModernSlavery.Core.Entities;
using System;

namespace ModernSlavery.WebUI.Submission.Presenters
{
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
    }
}
