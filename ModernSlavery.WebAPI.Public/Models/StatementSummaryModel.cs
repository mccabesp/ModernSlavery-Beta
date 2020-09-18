using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Classes.StatementTypeIndexes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.WebAPI.Public.Models
{
    public class StatementSummaryModelMapperProfile : Profile
    {
        public StatementSummaryModelMapperProfile()
        {
            CreateMap<Statement, StatementSummaryModel>()
                .ForMember(d => d.OrganisationName, opt => opt.MapFrom(s => s.Organisation.OrganisationName))
                .ForMember(d => d.OrganisationType, opt => opt.MapFrom(s => s.Organisation.SectorType))
                .ForMember(d => d.Address, opt => opt.MapFrom(s => s.Organisation.LatestAddress.GetAddressString(", ")))
                .ForMember(d => d.CompanyNumber, opt => opt.MapFrom(s => s.Organisation.CompanyNumber))

                .ForMember(d => d.GroupSubmission, opt => opt.MapFrom(s => s.StatementOrganisations.Any()))
                .ForMember(d => d.GroupOrganisations, opt => opt.MapFrom(st => st.StatementOrganisations.Select(s => s.OrganisationName)))

                .ForMember(d => d.ApprovedBy, s => s.MapFrom(s => s.ApprovingPerson))

                .ForMember(d => d.Sectors, opt => opt.MapFrom(st => st.Sectors.Select(s => s.StatementSectorType.Description).ToList()))
                .ForMember(d => d.Turnover, opt => opt.MapFrom(s => s.GetStatementTurnover()))

                .ForMember(d => d.Policies, opt => opt.MapFrom(st => st.Policies.Select(s => s.StatementPolicyType.Description).ToList()))

                .ForMember(d => d.RelevantRisks, opt => opt.MapFrom(st => st.RelevantRisks.Select(s => s.StatementRiskType.Description).ToList()))
                .ForMember(d => d.HighRisks, opt => opt.MapFrom(st => st.HighRisks.Select(s => s.StatementRiskType.Description).ToList()))
                .ForMember(d => d.LocationRisks, opt => opt.MapFrom(st => st.LocationRisks.Select(s => s.StatementRiskType.Description).ToList()))

                .ForMember(d => d.DueDiligences, opt => opt.MapFrom(st => st.Diligences.Select(s => s.StatementDiligenceType.Description).ToList()))
                .ForMember(d => d.HasForceLabour, opt => opt.MapFrom(st => !string.IsNullOrWhiteSpace(st.ForcedLabourDetails)))

                .ForMember(d => d.IncludesMeasuringProgress, opt => opt.MapFrom(st => (bool?)st.IncludesMeasuringProgress))
                .ForMember(d => d.HasSlaveryInstance, opt => opt.MapFrom(st => !string.IsNullOrWhiteSpace(st.SlaveryInstanceDetails)))
                .ForMember(d => d.HasRemediation, opt => opt.MapFrom(st => !string.IsNullOrWhiteSpace(st.SlaveryInstanceRemediation)))
                .ForMember(d => d.RemediationTypes, opt => opt.MapFrom(st => st.SlaveryInstanceRemediation.SplitI(Environment.NewLine, 0, StringSplitOptions.RemoveEmptyEntries).ToList()))

                .ForMember(d => d.Training, opt => opt.MapFrom(st => st.Training.Select(s => s.StatementTrainingType.Description).ToList()))

                .ForMember(d => d.StatementYears, opt => opt.MapFrom(s => s.GetStatementYears()));
        }
    }

    [Serializable]
    public class StatementSummaryModel
    {
        #region General Properties
        public DateTime SubmissionDeadline { get; set; }
        public string OrganisationName { get; set; }
        public SectorTypes OrganisationType { get; set; }
        public string Address { get; set; }
        public string CompanyNumber { get; set; }
        #endregion

        #region Group Submission
        public bool GroupSubmission { get; set; }
        public List<string> GroupOrganisations { get; set; } = new List<string>();
        #endregion

        #region Your Statement
        public string StatementUrl { get; set; }
        public DateTime StatementStartDate { get; set; }
        public DateTime StatementEndDate { get; set; }
        public string ApprovedBy { get; set; }
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
        public List<string> Sectors { get; set; } = new List<string>();

        public StatementTurnovers? Turnover { get; set; }
        #endregion

        #region Policies
        public List<string> Policies { get; set; } = new List<string>();

        #endregion

        #region Supply Chain Risks
        public List<string> RelevantRisks { get; set; } = new List<string>();
        public List<string> HighRisks { get; set; } = new List<string>();
        public List<string> LocationRisks { get; set; } = new List<string>();
        #endregion

        #region Due Diligence
        public List<string> DueDiligences { get; set; } = new List<string>();
        public bool? HasForceLabour { get; set; }
        public string ForcedLabourDetails { get; set; }
        public bool? HasSlaveryInstance { get; set; }
        public bool? HasRemediation { get; set; }
        public string SlaveryInstanceDetails { get; set; }
        public List<string> RemediationTypes { get; set; } = new List<string>();
        #endregion

        #region Training
        public List<string> Training { get; set; } = new List<string>();
        #endregion

        #region Monitoring progress
        public bool? IncludesMeasuringProgress { get; set; }
        public string ProgressMeasures { get; set; }
        public string KeyAchievements { get; set; }
        public StatementYears? StatementYears { get; set; }
        #endregion
    }
}