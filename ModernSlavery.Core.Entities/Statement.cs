using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public partial class Statement
    {
        public Statement()
        {
            Sectors = new HashSet<StatementSector>();
            Policies = new HashSet<StatementPolicy>();
            Training = new HashSet<StatementTraining>();
            RelevantRisks = new HashSet<StatementRisk>();
            Diligences = new HashSet<StatementDiligence>();
            HighRisks = new HashSet<StatementHighRisk>();
            LocationRisks = new HashSet<StatementLocationRisk>();
            StatusHistory = new HashSet<StatementStatus>();
        }

        #region Key statement control properties

        public long StatementId { get; set; }

        public long OrganisationId { get; set; }

        public virtual Organisation Organisation { get; set; }

        public DateTime SubmissionDeadline { get; set; }

        public long StatusId { get; set; }

        public virtual StatementStatus Status { get; set; }

        public virtual ICollection<StatementStatus> StatusHistory { get; set; }

        public DateTime StatusDate { get; set; }

        public string StatusDetails { get; set; }

        public DateTime Created { get; set; }

        public string LateReason { get; set; }

        public bool EHRCResponse { get; set; }

        #endregion

        #region Statement Page

        public string StatementUrl { get; set; }

        // Earliest date that the submission can be started
        public DateTime StatementStartDate { get; set; }

        // Latest date that the submission can be started
        public DateTime StatementEndDate { get; set; }

        public string ApproverFirstName { get; set; }

        public string ApproverLastName { get; set; }

        public string ApproverJobTitle { get; set; }

        public DateTime ApprovedDate { get; set; }

        #endregion

        #region Compliance Areas Covered Page

        //Organisation’s structure, business and supply chains
        public bool IncludesStructure { get; set; }

        public string StructureDetails { get; set; }

        public bool IncludesPolicies { get; set; }

        public string PolicyDetails { get; set; }

        //Risk assessment and management
        public bool IncludesRisks { get; set; }

        public string RisksDetails { get; set; }

        //Due diligence processes
        public bool IncludesDueDiligence { get; set; }

        public string DueDiligenceDetails { get; set; }

        //Staff training about slavery and human trafficking
        public bool IncludesTraining { get; set; }

        public string TrainingDetails { get; set; }

        //Goals and key performance indicators (KPIs)
        public bool IncludesGoals { get; set; }

        public string GoalsDetails { get; set; }

        #endregion

        #region Your organisation page

        public virtual ICollection<StatementSector> Sectors { get; set; }

        //HACK: Keep this for now as it may be required later since no provision in the UI
        public string OtherSector { get; set; }

        public decimal MinTurnover { get; set; }

        public decimal MaxTurnover { get; set; }

        #endregion

        #region Policies page

        public virtual ICollection<StatementPolicy> Policies { get; set; }

        public string OtherPolicies { get; set; }

        #endregion

        #region Supply chain risks and due diligence page 1

        //NOTE: Regions/countries can just be represented in the DB Parents/Child RelevantRisks
        public virtual ICollection<StatementRisk> RelevantRisks { get; set; }

        public string OtherRelevantRisks { get; set; }

        public virtual ICollection<StatementHighRisk> HighRisks { get; set; }

        public string OtherHighRisks { get; set; }

        public virtual ICollection<StatementLocationRisk> LocationRisks { get; set; }

        #endregion

        #region Supply chain risks and due diligence page 2

        //NOTE: I have added a new StatementDiligenceType.StatementDiligenceParentTypeId so we can have a hierarchy of DueDiligence to store Parnerships, Social Audits and Anonymous greievance mechanisms
        //NOTE: I have also added a new StatementDiligence.Description so we store "Other" categories of due diligence
        public virtual ICollection<StatementDiligence> Diligences { get; set; }

        public string ForcedLabourDetails { get; set; }

        public string SlaveryInstanceDetails { get; set; }

        //NOTE: The checkboxes and other can all be put into this field - each seperated by a newline character
        public string SlaveryInstanceRemediation { get; set; }

        #endregion

        #region Training Page

        public virtual ICollection<StatementTraining> Training { get; set; }

        public string OtherTraining { get; set; }

        #endregion

        #region Monitoring progress page

        public bool IncludesMeasuringProgress { get; set; }

        public string ProgressMeasures { get; set; }

        public string KeyAchievements { get; set; }

        public decimal MinStatementYears { get; set; }

        public decimal? MaxStatementYears { get; set; }

        #endregion

        public int IncludedOrganistionCount { get; set; }

        public int ExcludedOrganisationCount { get; set; }

    }
}
