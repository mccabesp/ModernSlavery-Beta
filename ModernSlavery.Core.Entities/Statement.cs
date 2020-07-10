using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public partial class Statement
    {
        public Statement()
        {
            Sectors = new HashSet<StatementSector>();
            Policies = new HashSet<StatementPolicy>();
            TrainingDivisions = new HashSet<StatementTrainingDivision>();
            Risks = new HashSet<StatementRisk>();
            Diligences = new HashSet<StatementDiligence>();
        }

        #region Key statement control properties

        public long StatementId { get; set; }

        public long OrganisationId { get; set; }
        public virtual Organisation Organisation { get; set; }

        // reporting deadline
        public DateTime SubmissionDeadline { get; set; }

        public StatementStatus Status { get; set; }

        // Date the status last changed
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

        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string JobTitle { get; set; }
        public DateTime ApprovedDate { get; set; }
        #endregion

        #region Compliance Areas Covered Page

        //Organisation’s structure, business and supply chains
        public bool IncludesStructure { get; set; }

        //TODO: Create StructureDetails
        //public string StructureDetails { get; set; }

        //Policies
        public bool IncludesPolicies { get; set; }

        //TODO: Create PolicyDetails
        //public string PolicyDetails { get; set; }

        //Risk assessment and management
        public bool IncludesRisks { get; set; }

        //TODO: Create RisksDetails
        //public string RisksDetails { get; set; }

        //TODO: Rename to IncludesDueDiligence
        //Due diligence processes
        public bool IncludesMethods { get; set; }

        //TODO: Create DueDiligenceDetails
        //public string DueDiligenceDetails { get; set; }

        //Staff training about slavery and human trafficking
        public bool IncludesTraining { get; set; }

        //TODO: Create TrainingDetails
        //public string TrainingDetails { get; set; }

        //TODO: Change type to boolean
        //Goals and key performance indicators (KPIs)
        public AffirmationType IncludesGoals { get; set; }

        //TODO: Create GoalsDetails
        //public string GoalsDetails { get; set; }

        //TODO: Remove - no longer needed
        [Obsolete("Remove - no longer needed")]
        public bool IncludesEffectiveness { get; set; }

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

        //TODO: Rename to OtherPolicies
        public string OtherPolicy { get; set; }

        #endregion

        #region Supply chain risks and due diligence page 1

        //TODO: Rename to OtherRelevantRisks
        public string OtherRisks { get; set; }

        //TODO: Rename to RelevantRisks and just put the relevent risks here. 
        //NOTE: Regions/countries can just be represented in the DB Parents/Child RelevantRisks
        public virtual ICollection<StatementRisk> Risks { get; set; }

        //TODO: Create another table for HighRisks
        //TODO: public virtual ICollection<StatementRisk> HighRisks { get; set; }

        //TODO: Create to OtherHighRisks
        //TODO: public string OtherHighRisks { get; set; }

        #endregion

        #region Supply chain risks and due diligence page 2
        //NOTE: I have added a new StatementDiligenceType.StatementDiligenceParentTypeId so we can have a hierarchy of DueDiligence to store Parnerships, Social Audits and Anonymous greievance mechanisms
        //NOTE: I have also added a new StatementDiligence.Description so we store "Other" categories of due diligence
        public virtual ICollection<StatementDiligence> Diligences { get; set; }

        //NOTE: Only complete when answer is yes
        //TODO: public string ForcedLabourDetails { get; set; }

        //NOTE: Only complete when answer is yes
        //TODO: public string SlaveryInstanceDetails { get; set; }

        //NOTE: The checkboxes and other can all be put into this field - each seperated by a newline character
        //TODO: public string SlaveryInstanceRemediation { get; set; }

        #endregion

        #region Training Page
        //TODO: Rename TrainingDivisions to TrainingTypes
        public virtual ICollection<StatementTrainingDivision> TrainingDivisions { get; set; }

        //TODO: Rename OtherTrainingDivision to OtherTrainingTypes
        public string OtherTrainingDivision { get; set; }

        #endregion

        #region Monitoring progress page

        //TODO: Check with SamG if this new field is required or just a duplicate of IncludesGoals (see above) on Compliance Areas Covered page 
        //public bool IncludesMeasuringProgress { get; set; }

        //TODO: Rename to ProgressMeasures
        public string MeasuringProgress { get; set; }

        //TODO Create KeyAchievements
        public string KeyAchievements { get; set; }

        //TODO Create MinStatementYears with default of 0
        //public decimal MinStatementYears { get; set; }

        //TODO Create MaxStatementYears
        //public decimal? MaxStatementYears { get; set; }

        #endregion

        public int IncludedOrganistionCount { get; set; }

        public int ExcludedOrganisationCount { get; set; }

    }
}
