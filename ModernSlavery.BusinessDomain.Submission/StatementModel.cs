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

        public string ApproverJobTitle { get; set; }

        public string ApproverFirstName { get; set; }

        public string ApproverLastName { get; set; }

        public DateTime? ApprovedDate { get; set; }

        #endregion

        #region Step 2 - Compliance

        public bool? IncludesStructure { get; set; }

        public string StructureDetails { get; set; }

        public bool? IncludesPolicies { get; set; }

        public string PolicyDetails { get; set; }

        public bool? IncludesRisks { get; set; }

        public string RisksDetails { get; set; }

        public bool? IncludesDueDiligence { get; set; }

        public string DueDiligenceDetails { get; set; }

        public bool? IncludesTraining { get; set; }

        public string TrainingDetails { get; set; }

        public bool? IncludesGoals { get; set; }

        public string GoalsDetails { get; set; }

        #endregion

        #region Step 3 - Your organisation

        public List<KeyValuePair<short, string>> StatementSectors { get; set; }

        public string OtherSector { get; set; }

        public int? MinTurnover { get; set; }

        public int? MaxTurnover { get; set; }

        #endregion

        #region Step 4 - Policies

        public List<KeyValuePair<int, string>> StatementPolicies { get; set; }

        public string OtherPolicies { get; set; }

        #endregion

        #region Step 5 - Supply chain risks and due diligence part 1

        public List<KeyValuePair<int, string>> RelevantRisks { get; set; }

        public string OtherRelevantRisks { get; set; }

        public List<KeyValuePair<int, string>> HighRisks { get; set; }

        public string OtherHighRisks { get; set; }

        public List<KeyValuePair<int, string>> LocationRisks { get; set; }

        #endregion

        #region Step 5 - Supply chain risks and due diligence part 2

        public List<KeyValuePair<int, string>> Diligences { get; set; }

        public string ForcedLabourDetails { get; set; }

        public string SlaveryInstanceDetails { get; set; }

        public string SlaveryInstanceRemediation { get; set; }

        #endregion

        #region Step 6 - Training

        public List<KeyValuePair<int, string>> Training { get; set; }

        public string OtherTraining { get; set; }

        #endregion

        #region Step 7 - Monitoring progress

        public bool IncludesMeasuringProgress { get; set; }

        public string ProgressMeasures { get; set; }

        public string KeyAchievements { get; set; }

        public decimal MinStatementYears { get; set; }

        public decimal? MaxStatementYears { get; set; }

        #endregion
    }
}
