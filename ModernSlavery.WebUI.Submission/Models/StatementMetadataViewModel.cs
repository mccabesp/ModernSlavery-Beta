using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using System;
using System.Collections.Generic;
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
        [MaxLength(255, ErrorMessage = "The web address (URL) cannot be longer than 255 characters.")]
        [Display(Name = "URL")]
        public string StatementUrl { get; set; }
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public DateTime ApprovedDate { get; set; }
        public AffirmationType IncludesGoals { get; set; }
        [Required]
        public bool IncludesStructure { get; set; }

        public string IncludesStructureDetail { get; set; }
        [Required]
        public bool IncludesPolicies { get; set; }
        public string IncludesPoliciesDetail { get; set; }
        [Required]
        public bool IncludesMethods { get; set; }
        public string IncludesMethodsDetail { get; set; }
        [Required]
        public bool IncludesRisks { get; set; }
        public string IncludesRisksDetail { get; set; }
        [Required]
        public bool IncludesEffectiveness { get; set; }

        public string IncludedEffectivenessDetail { get; set; }
        [Required]
        public bool IncludesTraining { get; set; }
        public string IncludesTrainingDetail { get; set; }

        public int IncludedOrganistionCount { get; set; }

        public int ExcludedOrganisationCount { get; set; }
    }
}
