using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Classes.StatementTypeIndexes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.WebUI.Viewing.Models
{
    public class StatementViewModelMapperProfile : Profile
    {
        public StatementViewModelMapperProfile()
        {
            CreateMap<StatementModel.RisksModel, StatementViewModel.RiskViewModel>();
            CreateMap<StatementModel.DiligenceModel, StatementViewModel.DueDiligenceViewModel>();

            CreateMap<StatementModel, StatementViewModel>();
        }
    }

    [Serializable]
    public class StatementViewModel
    {
        [IgnoreMap]
        public SectorTypeIndex SectorTypes { get; }
        [IgnoreMap]
        public PolicyTypeIndex PolicyTypes { get; }
        [IgnoreMap]
        public RiskTypeIndex RiskTypes { get; }
        [IgnoreMap]
        public DiligenceTypeIndex DiligenceTypes { get; }
        [IgnoreMap]
        public TrainingTypeIndex TrainingTypes { get; }

        public StatementViewModel(SectorTypeIndex sectorTypes, PolicyTypeIndex policyTypes, RiskTypeIndex riskTypes, DiligenceTypeIndex diligenceTypes, TrainingTypeIndex trainingTypes)
        {
            SectorTypes = sectorTypes;
            PolicyTypes = policyTypes;
            RiskTypes = riskTypes;
            DiligenceTypes = diligenceTypes;
            TrainingTypes = trainingTypes;
        }

        #region Types
        public class RiskViewModel
        {
            public short Id { get; set; }
            public string Details { get; set; }
        }
        public class DueDiligenceViewModel
        {
            public short Id { get; set; }
            public string Details { get; set; }
        }
        public enum TurnoverRanges : byte
        {
            //Not Provided
            NotProvided = 0,

            [Display(Description = "Under £36 million")]
            Under36Million = 1,

            [Display(Description = "£36 million to £60 million")]
            From36to60Million = 2,

            [Display(Description = "£60 million to £100 million")]
            From60to100Million = 3,

            [Display(Description = "£100 million to £500 million")]
            From100to500Million = 4,

            [Display(Description = "Over £500 million")]
            Over500Million = 5,
        }

        public enum YearRanges : byte
        {
            NotProvided = 0,

            [Display(Description = "This is the first time")]
            Year1 = 1,

            [Display(Description = "1 to 5 years")]
            Years1To5 = 2,

            [Display(Description="More than 5 years")]
            Over5Years = 3,
        }
        #endregion

        #region General Properties
        public long StatementId { get; set; }
        public DateTime StatementDeadline { get; set; }
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public string EncryptedOrganisationId { get; set; }
        public SectorTypes SectorType { get; set; }
        public DateTime Modified { get; set; }
        public string ReturnUrl { get; set; }
        public bool IsVoluntarySubmission { get; set; }
        public bool IsLateSubmission { get; set; }
        public bool ShouldProvideLateReason { get; set; }
        public bool IsInScopeForThisReportingYear { get; set; }
        #endregion

        #region Your Statement
        public string StatementUrl { get; set; }
        public DateTime StatementStartDate { get; set; }
        public DateTime StatementEndDate { get; set; }
        public string Approver { get; set; }
        public DateTime ApprovedDate { get; set; }
        #endregion

        #region Compliance
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

        #region Your Organisation
        public List<short> Sectors { get; set; } = new List<short>();

        public string OtherSector { get; set; }

        public TurnoverRanges? Turnover { get; set; }
        #endregion

        #region Policies
        public List<short> Policies { get; set; } = new List<short>();

        public string OtherPolicies { get; set; }

        #endregion

        #region Supply Chain Risks
        public List<RiskViewModel> RelevantRisks { get; set; } = new List<RiskViewModel>();
        public string OtherRelevantRisks;
        public List<RiskViewModel> HighRisks { get; set; } = new List<RiskViewModel>();
        public string OtherHighRisks;
        public List<RiskViewModel> LocationRisks { get; set; } = new List<RiskViewModel>();
        #endregion

        #region Due Diligence
        public List<DueDiligenceViewModel> DueDiligences { get; set; } = new List<DueDiligenceViewModel>();
        public bool? HasForceLabour { get; set; }
        public string ForcedLabourDetails { get; set; }
        public bool? HasSlaveryInstance { get; set; }
        public bool? HasRemediation { get; set; }
        public string SlaveryInstanceDetails { get; set; }

        [IgnoreMap]
        public string[] RemediationTypes = new[]
        {
            "repayment of recruitment fees",
            "change in policy",
            "referring victims to government services",
            "supporting victims via NGOs",
            "supporting criminal justice against perpetrator",
            "other"
        };

        [IgnoreMap]
        public List<string> SelectedRemediationTypes { get; set; } = new List<string>();

        [IgnoreMap]
        public string OtherRemediation { get; set; }

        public string SlaveryInstanceRemediation
        {
            get
            {
                var selectedRemediationTypes = new List<string>(SelectedRemediationTypes.Where(s => !string.IsNullOrWhiteSpace(s)));

                if (selectedRemediationTypes.Contains("other"))
                {
                    selectedRemediationTypes.Remove("other");
                    selectedRemediationTypes.Add(OtherRemediation);
                }
                return selectedRemediationTypes.ToDelimitedString(Environment.NewLine);
            }
            set
            {
                var selectedRemediationTypes = new List<string>(value.SplitI(Environment.NewLine).Where(s => !string.IsNullOrWhiteSpace(s)));

                //Set the selected types
                SelectedRemediationTypes.Clear();
                for (int i = selectedRemediationTypes.Count - 1; i >= 0; i--)
                {
                    if (RemediationTypes.ContainsI(selectedRemediationTypes[i]))
                    {
                        SelectedRemediationTypes.Add(selectedRemediationTypes[i]);
                        selectedRemediationTypes.RemoveAt(i);
                    }
                }
                OtherRemediation = selectedRemediationTypes.ToDelimitedString(Environment.NewLine);
                if (!string.IsNullOrWhiteSpace(OtherRemediation)) SelectedRemediationTypes.Add("other");
            }
        }

        #endregion

        #region Training
        public List<short> Training { get; set; } = new List<short>();
        public string OtherTraining { get; set; }
        #endregion

        #region Monitoring progress
        public bool? IncludesMeasuringProgress { get; set; }
        public string ProgressMeasures { get; set; }
        public string KeyAchievements { get; set; }
        public YearRanges? StatementYears { get; set; }
        #endregion
    }
}