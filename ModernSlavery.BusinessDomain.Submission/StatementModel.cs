using AutoMapper;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModernSlavery.BusinessDomain.Submission
{
    public class StatementMapperProfile : Profile
    {
        public StatementMapperProfile()
        {
            // name things same as DB
            CreateMap<Statement, StatementModel>()
                .ForMember(dest => dest.Status, opt => opt.Ignore()) // TODO - James Map this appropriately
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.SubmissionDeadline.Year))
                .ForMember(dest => dest.StatementSectors, opt => opt.MapFrom(src => src.Sectors.Select(s => s.StatementSectorTypeId)))
                .ForMember(dest => dest.StatementPolicies, opt => opt.MapFrom(src => src.Policies.Select(p => p.StatementPolicyTypeId)))
                .ForMember(dest => dest.Training, opt => opt.MapFrom(src => src.Training.Select(t => t.StatementTrainingTypeId)))
                .ForMember(dest => dest.RelevantRisks, opt => opt.MapFrom(src => src.RelevantRisks.Select(r => r.StatementRiskTypeId)))
                .ForMember(dest => dest.HighRisks, opt => opt.MapFrom(src => src.HighRisks.Select(r => r.StatementRiskTypeId)))
                .ForMember(dest => dest.LocationRisks, opt => opt.MapFrom(src => src.LocationRisks.Select(r => r.StatementRiskTypeId)))
                .ForMember(dest => dest.Diligences, opt => opt.MapFrom(src => src.Diligences.Select(d => d.StatementDiligenceTypeId)));
        }
    }

    [Serializable]
    public class StatementModel
    {
        public long UserId { get; set; }
        public DateTime Timestamp { get; set; }

        public long? StatementId { get; set; }
        public DateTime? BackupDate { get; set; }

        public StatementStatuses Status { get; set; }

        public DateTime? StatusDate { get; set; }

        public DateTime SubmissionDeadline { get; set; }

        public long OrganisationId { get; set; }

        public int Year => SubmissionDeadline.Year;

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

        public List<short> StatementSectors { get; set; }

        public string OtherSector { get; set; }

        public int? MinTurnover { get; set; }

        public int? MaxTurnover { get; set; }

        #endregion

        #region Step 4 - Policies

        public List<short> StatementPolicies { get; set; }

        public string OtherPolicies { get; set; }

        #endregion

        #region Step 5 - Supply chain risks and due diligence part 1

        public List<(short StatementRiskTypeId,string Details)> RelevantRisks { get; set; }

        public string OtherRelevantRisks { get; set; }

        public List<(short StatementRiskTypeId, string Details)> HighRisks { get; set; }

        public string OtherHighRisks { get; set; }

        public List<(short StatementRiskTypeId, string Details)> LocationRisks { get; set; }

        #endregion

        #region Step 5 - Supply chain risks and due diligence part 2

        public List<(short StatementDiligenceTypeId, string Details)> Diligences { get; set; }

        public string ForcedLabourDetails { get; set; }

        public string SlaveryInstanceDetails { get; set; }

        public string SlaveryInstanceRemediation { get; set; }

        #endregion

        #region Step 6 - Training

        public List<(short StatementTrainingTypeId, string Details)> Training { get; set; }

        public string OtherTraining { get; set; }

        #endregion

        #region Step 7 - Monitoring progress

        public bool? IncludesMeasuringProgress { get; set; }

        public string ProgressMeasures { get; set; }

        public string KeyAchievements { get; set; }

        public decimal? MinStatementYears { get; set; }

        public decimal? MaxStatementYears { get; set; }

        #endregion
    }
}
