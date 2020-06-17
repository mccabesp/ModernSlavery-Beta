using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModernSlavery.Core.Entities;
using System;
using System.ComponentModel.DataAnnotations;

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

        public DateTime ReportingEndDate { get; set; }
        public DateTime AccountingDate { get; set; }
        // This should never go over the wire!
        public long OrganisationId { get; set; }

        public string OrganisationIdentifier { get; set; }

        public int Year { get; set; }
        [Url]
        [Display(Name = "URL")]
        public string StatementUrl { get; set; }
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public DateTime ApprovedDate { get; set; }

    }
}
