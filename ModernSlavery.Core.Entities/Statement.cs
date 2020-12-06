using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Policy;
using System.Text;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Entities
{
    public partial class Statement
    {
        #region Constructors
        public Statement()
        {
            StatementOrganisations = new HashSet<StatementOrganisation>();
            Sectors = new HashSet<StatementSector>();
            Statuses = new HashSet<StatementStatus>();
        }
        #endregion

        #region Statement Key Fields
        public long StatementId { get; set; }
        public long OrganisationId { get; set; }
        public virtual Organisation Organisation { get; set; }
        public DateTime SubmissionDeadline { get; set; }
        #endregion

        #region Statement Status Fields
        public bool IsLateSubmission { get; set; }
        public string LateReason { get; set; }

        public StatementStatuses Status { get; set; }
        public virtual ICollection<StatementStatus> Statuses { get; set; }
        public DateTime StatusDate { get; set; }
        public string StatusDetails { get; set; }
        #endregion

        #region Statement Control Fields
        public string Modifications { get; set; }
        public DateTime Modified { get; set; } = VirtualDateTime.Now;
        public DateTime Created { get; set; } = VirtualDateTime.Now;
        #endregion

        #region Group Organisation Fields
        public virtual ICollection<StatementOrganisation> StatementOrganisations { get; set; }

        #endregion

        #region Url & Email Fields
        public string StatementUrl { get; set; }
        public string StatementEmail { get; set; }
        #endregion

        #region Statement Period Fields
        // Earliest date that the submission can be started
        public DateTime StatementStartDate { get; set; }

        // Latest date that the submission can be started
        public DateTime StatementEndDate { get; set; }

        #endregion

        #region Approver & Date Fields
        public string ApproverFirstName { get; set; }

        public string ApproverLastName { get; set; }

        public string ApproverJobTitle { get; set; }

        public DateTime ApprovedDate { get; set; }

        #endregion

        #region Compliance Fields
        public bool IncludesStructure { get; set; }
        public string StructureDetails { get; set; }

        public bool IncludesPolicies { get; set; }
        public string PolicyDetails { get; set; }

        public bool IncludesRisks { get; set; }
        public string RisksDetails { get; set; }

        public bool IncludesDueDiligence { get; set; }
        public string DueDiligenceDetails { get; set; }

        public bool IncludesTraining { get; set; }
        public string TrainingDetails { get; set; }

        public bool IncludesGoals { get; set; }
        public string GoalsDetails { get; set; }
        #endregion

        #region Sectors Fields
        public virtual ICollection<StatementSector> Sectors { get; set; }

        public string OtherSectors { get; set; }

        #endregion

        #region Turnover Fields
        public enum StatementTurnoverRanges : byte
        {
            //Not Provided
            [Description("Not provided")]
            [Range(0, 0)]
            NotProvided = 0,

            //Under £36 million
            [Description("Under £36 million")]
            [Range(0, 36)]
            Under36Million = 1,

            //£36 million - £60 million
            [Description("£36 million to £60 million")]
            [Range(36, 60)]
            From36to60Million = 2,

            //£60 million - £100 million
            [Description("£60 million to £100 million")]
            [Range(60, 100)]
            From60to100Million = 3,

            //£100 million - £500 million
            [Description("£100 million to £500 million")]
            [Range(100, 500)]
            From100to500Million = 4,

            //£500 million+
            [Description("Over £500 million")]
            [Range(500, 0)]
            Over500Million = 5,
        }

        public StatementTurnoverRanges Turnover { get; set; }
        #endregion

        #region Statement Years Fields
        public enum StatementYearRanges : byte
        {
            //Not Provided
            [Description("Not provided")]
            [Range(0, 0)]
            NotProvided = 0,

            //This is the first time
            [Description("This is the first time")]
            [Range(1, 1)]
            Year1 = 1,

            //1 to 5 Years
            [Description("1 to 5 years")]
            [Range(1, 5)]
            Years1To5 = 2,

            //More than 5 years
            [Description("More than 5 years")]
            [Range(5, 0)]
            Over5Years = 3,
        }

        public StatementYearRanges StatementYears { get; set; }
        #endregion

        #region Statement Summary Fields
        public virtual StatementSummary.V1.StatementSummary Summary { get; set; }
        #endregion
    }
}
