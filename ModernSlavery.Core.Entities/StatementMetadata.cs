using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public partial class StatementMetadata
    {
        public long StatementMetadataId { get; set; }

        public long OrganisationId { get; set; }

        // Earliest date that the submission can be started
        public DateTime ReportingStartDate { get; set; }

        // Latest date that the submission can be started
        public DateTime ReportingEndDate { get; set; }

        // reporting deadline
        public DateTime AccountingDate { get; set; }

        public string StatementUrl { get; set; }

        public AffirmationType IncludesGoals { get; set; }

        public bool IncludesStructure { get; set; }

        public bool IncludesPolicies { get; set; }

        public bool IncludesMethods { get; set; }

        public bool IncludesRisks { get; set; }

        public bool IncludesEffectiveness { get; set; }

        public bool IncludesTraining { get; set; }

        public int IncludedOrganistionCount { get; set; }

        public int ExcludedOrganisationCount { get; set; }

        public ReturnStatuses Status { get; set; }

        // Date the status last changed
        public DateTime StatusDate { get; set; }

        public string StatusDetails { get; set; }

        public DateTime Created { get; set; }

        public string JobTitle { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime ApprovedDate { get; set; }

        public decimal MinTurnover { get; set; }

        public decimal MaxTurnover { get; set; }

        public string LateReason { get; set; }

        public string OtherTrainingDivision { get; set; }

        public string OtherPolicy { get; set; }

        public string OtherRisk { get; set; }

        public string OtherSector { get; set; }

        public string MeasuringProgress { get; set; }

        public bool EHRCResponse { get; set; }

        public virtual Organisation Organisation { get; set; }
    }
}
