using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Classes.StatementTypeIndexes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Entities.StatementSummary;
using ModernSlavery.Core.Extensions;
using static ModernSlavery.Core.Entities.Statement;

namespace ModernSlavery.WebUI.Viewing.Models
{
    public class StatementViewModelMapperProfile : Profile
    {
        public StatementViewModelMapperProfile()
        {
            CreateMap<StatementModel, StatementViewModel>();
        }
    }

    [Serializable]
    public class StatementViewModel
    {
        [IgnoreMap]
        public SectorTypeIndex SectorTypes { get; }

        public StatementViewModel(SectorTypeIndex sectorTypes)
        {
            SectorTypes = sectorTypes;
        }

        public StatementViewModel()
        {

        }

        #region Statement Key Fields
        public long StatementId { get; set; }
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public DateTime SubmissionDeadline { get; set; }
        #endregion

        #region Organisation Fields
        [IgnoreMap]public string OrganisationAddress { get; set; }
        [IgnoreMap]public string EncryptedOrganisationId { get; set; }
        [IgnoreMap]public SectorTypes SectorType { get; set; }
        #endregion

        #region Statement Status Fields
        #endregion

        #region Statement Control Fields
        public DateTime Modified { get; set; }
        #endregion

        #region Url & Email Fields
        public string StatementUrl { get; set; }
        public string StatementEmail { get; set; }
        #endregion

        #region Statement Period Fields
        public DateTime StatementStartDate { get; set; }
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
        public List<short> Sectors { get; set; } = new List<short>();
        public string OtherSectors { get; set; }
        #endregion

        #region Turnover Fields
        public StatementTurnoverRanges Turnover { get; set; }
        #endregion

        #region Statement Years Fields
        public StatementYearRanges StatementYears { get; set; }
        #endregion

        #region Statement Summary Fields
        public StatementSummary1 Summary { get; set; }
        #endregion

        #region Navigation Properties
        [IgnoreMap]
        public string ReturnUrl { get; set; }
        #endregion

        #region Calculated Properties
        [IgnoreMap]
        public bool IsVoluntarySubmission { get; set; }
        [IgnoreMap]
        public bool IsLateSubmission { get; set; }
        [IgnoreMap]
        public bool ShouldProvideLateReason { get; set; }
        [IgnoreMap]
        public bool IsInScopeForThisReportingYear { get; set; }
        #endregion
    }
}