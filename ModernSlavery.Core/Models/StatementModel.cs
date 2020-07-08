using ModernSlavery.Core.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ModernSlavery.Core.Models
{
    [Serializable]
    public class StatementModel
    {
        public long? StatementId { get; set; }

        public DateTime? StatusDate { get; set; }

        public DateTime? StatementStartDate { get; set; }

        public DateTime? StatementEndDate { get; set; }

        public DateTime SubmissionDeadline { get; set; }

        public long OrganisationId { get; set; }

        public int Year { get; set; }

        [MaxLength(255)]
        public string StatementUrl { get; set; }

        public string JobTitle { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool IncludesGoals { get; set; }

        public bool IncludesStructure { get; set; }

        public string IncludesStructureDetail { get; set; }

        public bool IncludesPolicies { get; set; }

        public string IncludesPoliciesDetail { get; set; }

        public bool IncludesMethods { get; set; }

        public string IncludesMethodsDetail { get; set; }

        public bool IncludesRisks { get; set; }

        public string IncludesRisksDetail { get; set; }

        public bool IncludesEffectiveness { get; set; }

        public string IncludedEffectivenessDetail { get; set; }

        public bool IncludesTraining { get; set; }

        public string IncludesTrainingDetail { get; set; }

        public int IncludedOrganistionCount { get; set; }

        public int ExcludedOrganisationCount { get; set; }

        public string OtherSectorText { get; set; }

        public List<StatementSector> StatementSectors { get; set; }

        public List<StatementPolicy> StatementPolicies { get; set; }

        public string OtherPolicyText { get; set; }

        public List<StatementTrainingDivision> StatementTrainingDivisions { get; set; }

        public string OtherTrainingText { get; set; }

        public List<StatementRisk> StatementRisks { get; set; }

        public List<StatementRiskType> StatementRiskTypes { get; set; }

        public string OtherRiskText { get; set; }

        public List<Continent> Continents { get; set; }

        public List<Core.Classes.Country> Countries { get; set; }

        public List<StatementDiligence> StatementDiligences { get; set; }

        public List<StatementDiligenceType> StatementDiligenceTypes { get; set; }

        public string IndicatorDetails { get; set; }

        public string InstanceDetails { get; set; }

        public string OtherRemediationText { get; set; }

        [MaxLength(500)]
        public string MeasuringProgress { get; set; }

        [MaxLength(500)]
        public string KeyAchievements { get; set; }

        // turnover? Relational types?
    }
}
