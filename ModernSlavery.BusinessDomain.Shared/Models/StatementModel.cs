using AutoMapper;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ModernSlavery.BusinessDomain.Shared.Models
{
    public class StatementModelMapperProfile : Profile
    {
        public StatementModelMapperProfile()
        {
            CreateMap<StatementModel, Statement>()
                .ForMember(d => d.MinStatementYears, opt => opt.MapFrom(s => s.StatementYears.GetAttribute<RangeAttribute>().Minimum))
                .ForMember(d => d.MaxStatementYears, opt => opt.MapFrom(s => s.StatementYears.GetAttribute<RangeAttribute>().Maximum))
                .ForMember(d => d.MinTurnover, opt => opt.MapFrom(s => s.Turnover.GetAttribute<RangeAttribute>().Minimum))
                .ForMember(d => d.MaxTurnover, opt => opt.MapFrom(s => s.Turnover.GetAttribute<RangeAttribute>().Maximum))
                .ForMember(dest => dest.Sectors, opt => opt.Ignore())
                .ForMember(dest => dest.Policies, opt => opt.Ignore())
                .ForMember(dest => dest.RelevantRisks, opt => opt.Ignore())
                .ForMember(dest => dest.HighRisks, opt => opt.Ignore())
                .ForMember(dest => dest.LocationRisks, opt => opt.Ignore())
                .ForMember(dest => dest.Diligences, opt => opt.Ignore())
                .ForMember(dest => dest.Training, opt => opt.Ignore())
                .ForMember(dest => dest.StatementId, opt => opt.Ignore())
                .ForMember(dest => dest.Created, opt => opt.Ignore())
                .ForMember(dest => dest.Modified, opt => opt.Ignore())
                .ForMember(dest => dest.Organisation, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.StatusDate, opt => opt.Ignore())
                .ForMember(dest => dest.StatusDetails, opt => opt.Ignore())
                .ForMember(dest => dest.Statuses, opt => opt.Ignore());

            CreateMap<Statement, StatementModel>()
                .ForMember(d => d.Submitted, opt => opt.MapFrom(s => s.StatementId>0))
                .ForMember(d => d.StatementStartDate, opt => opt.MapFrom(s => s.StatementStartDate == DateTime.MinValue ? (DateTime?)null : s.StatementStartDate))
                .ForMember(d => d.StatementEndDate, opt => opt.MapFrom(s => s.StatementEndDate == DateTime.MinValue ? (DateTime?)null : s.StatementEndDate))
                .ForMember(d => d.ApprovedDate, opt => opt.MapFrom(s => s.ApprovedDate == DateTime.MinValue ? (DateTime?)null : s.ApprovedDate))
                .ForMember(d => d.OrganisationName, opt => opt.MapFrom(s => s.Organisation.OrganisationName))
                .ForMember(d => d.StatementYears, opt => opt.MapFrom(s => Enums.GetEnumFromRange<StatementModel.YearRanges>(s.MinStatementYears, s.MaxStatementYears == null ? 0 : s.MaxStatementYears.Value)))
                .ForMember(d => d.Turnover, opt => opt.MapFrom(s => Enums.GetEnumFromRange<StatementModel.TurnoverRanges>(s.MinTurnover, s.MaxTurnover == null ? 0 : s.MaxTurnover.Value)))
                .ForMember(dest => dest.Modifications, opt => opt.MapFrom(s=>string.IsNullOrWhiteSpace(s.Modifications) ? null : JsonConvert.DeserializeObject<List<AutoMap.Diff>>(s.Modifications)))
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Sectors, opt => opt.MapFrom(st => st.Sectors.Select(s => s.StatementSectorTypeId)))
                .ForMember(dest => dest.Policies, opt => opt.MapFrom(st => st.Policies.Select(s => s.StatementPolicyTypeId)))
                .ForMember(dest => dest.RelevantRisks, opt => opt.MapFrom(st => st.RelevantRisks.Select(s => new StatementModel.RisksModel { Id = s.StatementRiskTypeId, Details = s.Details })))
                .ForMember(dest => dest.HighRisks, opt => opt.MapFrom(st => st.HighRisks.Select(s => new StatementModel.RisksModel { Id = s.StatementRiskTypeId, Details = s.Details })))
                .ForMember(dest => dest.LocationRisks, opt => opt.MapFrom(st => st.LocationRisks.Select(s => new StatementModel.RisksModel { Id = s.StatementRiskTypeId, Details = s.Details })))
                .ForMember(dest => dest.DueDiligences, opt => opt.MapFrom(st => st.Diligences.Select(s => new StatementModel.DiligenceModel { Id = s.StatementDiligenceTypeId, Details = s.Details })))
                .ForMember(dest => dest.Training, opt => opt.MapFrom(st => st.Training.Select(s => s.StatementTrainingTypeId)))
                .ForMember(dest => dest.EditorUserId, opt => opt.Ignore())
                .ForMember(dest => dest.EditTimestamp, opt => opt.Ignore())
                .ForMember(dest => dest.DraftBackupDate, opt => opt.Ignore())
                .ForMember(dest => dest.ReturnToReviewPage, opt => opt.MapFrom(st=>st.StatementId>0))
                // for nullable to non-nullable properties, dont overwrite the null with the default values when new
                .ForMember(dest => dest.IncludesStructure, opt => opt.MapFrom(st => st.StatementId == 0 ? null : (bool?)st.IncludesStructure))
                .ForMember(dest => dest.IncludesPolicies, opt => opt.MapFrom(st => st.StatementId == 0 ? null : (bool?)st.IncludesPolicies))
                .ForMember(dest => dest.IncludesRisks, opt => opt.MapFrom(st => st.StatementId == 0 ? null : (bool?)st.IncludesRisks))
                .ForMember(dest => dest.IncludesDueDiligence, opt => opt.MapFrom(st => st.StatementId == 0 ? null : (bool?)st.IncludesDueDiligence))
                .ForMember(dest => dest.IncludesTraining, opt => opt.MapFrom(st => st.StatementId == 0 ? null : (bool?)st.IncludesTraining))
                .ForMember(dest => dest.IncludesGoals, opt => opt.MapFrom(st => st.StatementId == 0 ? null : (bool?)st.IncludesGoals))
                .ForMember(dest => dest.IncludesMeasuringProgress, opt => opt.MapFrom(st => st.StatementId == 0 ? null : (bool?)st.IncludesMeasuringProgress))
                .ForMember(dest => dest.HasForceLabour, opt => opt.MapFrom(st => st.StatementId == 0 ? (bool?)null : !string.IsNullOrWhiteSpace(st.ForcedLabourDetails)))
                .ForMember(dest => dest.HasSlaveryInstance, opt => opt.MapFrom(st => st.StatementId == 0 ? (bool?)null : !string.IsNullOrWhiteSpace(st.SlaveryInstanceDetails)))
                .ForMember(dest => dest.HasRemediation, opt => opt.MapFrom(st => st.StatementId == 0 ? (bool?)null : !string.IsNullOrWhiteSpace(st.SlaveryInstanceRemediation)));
        }
    }

    [Serializable]
    public class StatementModel
    {
        #region Types
        public enum TurnoverRanges : byte
        {
            //Not Provided
            [Range(0, 0)]
            NotProvided = 0,

            //Under £36 million
            [Range(0, 36)]
            Under36Million = 1,

            //£36 million - £60 million
            [Range(36, 60)]
            From36to60Million = 2,

            //£60 million - £100 million
            [Range(60, 100)]
            From60to100Million = 3,

            //£100 million - £500 million
            [Range(100, 500)]
            From100to500Million = 4,

            //£500 million+
            [Range(500, 0)]
            Over500Million = 5,
        }

        public enum YearRanges : byte
        {
            //Not Provided
            [Range(0, 0)]
            NotProvided = 0,

            //This is the first time
            [Range(1, 1)]
            Year1 = 1,

            //1 to 5 Years
            [Range(1, 5)]
            Years1To5 = 2,

            //More than 5 years
            [Range(5, 0)]
            Over5Years = 3,
        }
        #endregion

        public bool Submitted { get; set; }
        public bool ReturnToReviewPage { get; set; }
        public DateTime? DraftBackupDate { get; set; }

        public long EditorUserId { get; set; }
        public DateTime EditTimestamp { get; set; }

        public long? StatementId { get; set; }

        public string OrganisationName { get; set; }

        public StatementStatuses Status { get; set; }

        public DateTime? StatusDate { get; set; }

        public DateTime SubmissionDeadline { get; set; }

        public long OrganisationId { get; set; }

        [JsonIgnore]
        [IgnoreMap]
        public IList<AutoMap.Diff> Modifications { get; set; }
        public bool EHRCResponse { get; set; }
        public string LateReason { get; set; }
        public short IncludedOrganisationCount { get; set; }

        public short ExcludedOrganisationCount { get; set; }
        public DateTime Modified { get; set; } = VirtualDateTime.Now;
        public DateTime Created { get; set; } = VirtualDateTime.Now;

        #region Step 1 - your statement

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
        public SortedSet<short> Sectors { get; set; } = new SortedSet<short>();

        public string OtherSector { get; set; }

        public TurnoverRanges Turnover;

        #endregion

        #region Step 4 - Policies

        public SortedSet<short> Policies { get; set; } = new SortedSet<short>();

        public string OtherPolicies { get; set; }

        #endregion

        #region Step 5 - Supply chain risks and due diligence part 1

        public class RisksModel
        {
            public short Id { get; set; }
            public string Details { get; set; }
        }

        public List<RisksModel> RelevantRisks { get; set; } = new List<RisksModel>();

        public string OtherRelevantRisks { get; set; }

        public List<RisksModel> HighRisks { get; set; } = new List<RisksModel>();

        public string OtherHighRisks { get; set; }

        public List<RisksModel> LocationRisks { get; set; } = new List<RisksModel>();

        #endregion

        #region Step 5 - Supply chain risks and due diligence part 2
        public class DiligenceModel
        {
            public short Id { get; set; }
            public string Details { get; set; }
        }

        public List<DiligenceModel> DueDiligences { get; set; } = new List<DiligenceModel>();

        public bool? HasForceLabour { get; set; }

        public bool? HasSlaveryInstance { get; set; }

        public bool? HasRemediation { get; set; }

        public string ForcedLabourDetails { get; set; }

        public string SlaveryInstanceDetails { get; set; }

        public string SlaveryInstanceRemediation { get; set; }

        #endregion

        #region Step 6 - Training

        public SortedSet<short> Training { get; set; } = new SortedSet<short>();

        public string OtherTraining { get; set; }

        #endregion

        #region Step 7 - Monitoring progress

        public bool? IncludesMeasuringProgress { get; set; }

        public string ProgressMeasures { get; set; }

        public string KeyAchievements { get; set; }

        public YearRanges StatementYears { get; set; }

        #endregion

        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(StatementUrl)
                && StatementStartDate == null
                && StatementEndDate == null
                && string.IsNullOrWhiteSpace(ApproverFirstName)
                && string.IsNullOrWhiteSpace(ApproverLastName)
                && string.IsNullOrWhiteSpace(ApproverJobTitle)
                && ApprovedDate == null
                && IncludesStructure == null
                && string.IsNullOrWhiteSpace(StructureDetails)
                && IncludesPolicies == null
                && string.IsNullOrWhiteSpace(PolicyDetails)
                && IncludesRisks == null
                && string.IsNullOrWhiteSpace(RisksDetails)
                && IncludesDueDiligence == null
                && string.IsNullOrWhiteSpace(DueDiligenceDetails)
                && IncludesTraining == null
                && string.IsNullOrWhiteSpace(TrainingDetails)
                && IncludesGoals == null
                && string.IsNullOrWhiteSpace(GoalsDetails)
                && (Sectors == null || !Sectors.Any())
                && string.IsNullOrWhiteSpace(OtherSector)
                && Turnover == TurnoverRanges.NotProvided
                && (Policies == null || !Policies.Any())
                && string.IsNullOrWhiteSpace(OtherPolicies)
                && (RelevantRisks == null || !RelevantRisks.Any())
                && string.IsNullOrWhiteSpace(OtherRelevantRisks)
                && (HighRisks == null || !HighRisks.Any())
                && string.IsNullOrWhiteSpace(OtherHighRisks)
                && (LocationRisks == null || !LocationRisks.Any())
                && (DueDiligences == null || !DueDiligences.Any())
                && string.IsNullOrWhiteSpace(ForcedLabourDetails)
                && string.IsNullOrWhiteSpace(SlaveryInstanceDetails)
                && string.IsNullOrWhiteSpace(SlaveryInstanceRemediation)
                && (Training == null || !Training.Any())
                && string.IsNullOrWhiteSpace(TrainingDetails)
                && IncludesMeasuringProgress == null
                && string.IsNullOrWhiteSpace(ProgressMeasures)
                && string.IsNullOrWhiteSpace(KeyAchievements)
                && StatementYears == YearRanges.NotProvided;

        }
    }
}
