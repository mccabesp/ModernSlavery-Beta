using ModernSlavery.Core.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ModernSlavery.BusinessDomain.Submission
{
    [Serializable]
    public class StatementModel
    {
        public long? StatementId { get; set; }

        public ReturnStatuses Status { get; set; }

        public DateTime? StatusDate { get; set; }

        public DateTime SubmissionDeadline { get; set; }

        public long OrganisationId { get; set; }

        public int Year { get; set; }

        #region Step 1 - your statement

        [MaxLength(255)]
        public string StatementUrl { get; set; }

        public DateTime? StatementStartDate { get; set; }

        public DateTime? StatementEndDate { get; set; }

        public string JobTitle { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? ApprovedDate { get; set; }

        #endregion

        #region Step 2 - Compliance

        public bool IncludesStructure { get; set; }

        public string IncludesStructureDetail { get; set; }

        public bool IncludesPolicies { get; set; }

        public string IncludesPoliciesDetail { get; set; }

        public bool IncludesRisks { get; set; }

        public string IncludesRisksDetail { get; set; }

        public bool IncludesEffectiveness { get; set; }

        public string IncludedEffectivenessDetail { get; set; }

        public bool IncludesTraining { get; set; }

        public string IncludesTrainingDetail { get; set; }

        public bool IncludesMethods { get; set; }

        public string IncludesMethodsDetail { get; set; }

        #endregion

        #region Step 3 - Your organisation

        public List<KeyValuePair<int, string>> StatementSectors { get; set; }

        public string OtherSectorText { get; set; }

        // turnover ?

        #endregion

        #region Step 4 - Policies

        public List<KeyValuePair<int, string>> StatementPolicies { get; set; }

        public string OtherPolicyText { get; set; }

        #endregion

        #region Step 5 - Supply chain risks and due diligence

        public List<KeyValuePair<int, string>> StatementDiligenceTypes { get; set; }

        public string OtherDiligenceText { get; set; }

        public List<KeyValuePair<int, string>> StatementRiskTypes { get; set; }

        public string OtherRiskText { get; set; }

        public List<KeyValuePair<int, string>> Countries { get; set; }

        #endregion

        #region Step 6 - Training

        public List<KeyValuePair<int, string>> StatementTrainingDivisions { get; set; }

        public string OtherTrainingText { get; set; }

        #endregion

        #region Step 7 - Monitoring progress

        public bool IncludesGoals { get; set; }

        [MaxLength(500)]
        public string MeasuringProgress { get; set; }

        [MaxLength(500)]
        public string KeyAchievements { get; set; }

        #endregion
    }
}
