using AutoMapper;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
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
                .ForMember(d => d.OrganisationName,opt=>opt.MapFrom(s=>s.Organisation.OrganisationName))
                .ForMember(d => d.StatementYears, opt => opt.MapFrom(s => Enums.GetEnumFromRange<StatementModel.YearRanges>((int)s.MinStatementYears, (int)s.MaxStatementYears)))
                .ForMember(d => d.Turnover, opt => opt.MapFrom(s => Enums.GetEnumFromRange<StatementModel.TurnoverRanges>((int)s.MinStatementYears, (int)s.MaxStatementYears)))
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Sectors, opt => opt.Ignore())
                .ForMember(dest => dest.Policies, opt => opt.Ignore())
                .ForMember(dest => dest.RelevantRisks, opt => opt.Ignore())
                .ForMember(dest => dest.HighRisks, opt => opt.Ignore())
                .ForMember(dest => dest.LocationRisks, opt => opt.Ignore())
                .ForMember(dest => dest.DueDiligences, opt => opt.Ignore())
                .ForMember(dest => dest.Training, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Timestamp, opt => opt.Ignore())
                .ForMember(dest => dest.BackupDate, opt => opt.Ignore())
                .ForMember(dest => dest.CanRevertToBackup, opt => opt.Ignore());
        }
    }

    [Serializable]
    public class StatementModel
    {
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

        public bool CanRevertToBackup { get; set; }

        public long UserId { get; set; }
        public DateTime Timestamp { get; set; }

        public long? StatementId { get; set; }

        public string OrganisationName { get; set; }

        public DateTime? BackupDate { get; set; }

        public StatementStatuses Status { get; set; }

        public DateTime? StatusDate { get; set; }

        public DateTime SubmissionDeadline { get; set; }

        public long OrganisationId { get; set; }

        public string Modifications { get; set; }
        public string EHRCResponse { get; set; }
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
        public class SectorModel
        {
            public SectorModel(short id, string description, bool isSelected)
            {
                Id = id;
                Description = description;
                IsSelected = isSelected;
            }

            public short Id { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
        }

        public List<SectorModel> Sectors { get; set; }

        public string OtherSector { get; set; }

        public TurnoverRanges Turnover;

        #endregion

        #region Step 4 - Policies
        public class PolicyModel
        {
            public PolicyModel(short id, string description, bool isSelected)
            {
                Id = id;
                Description = description;
                IsSelected = isSelected;
            }
            public short Id { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
        }

        public List<PolicyModel> Policies { get; set; }

        public string OtherPolicies { get; set; }

        #endregion

        #region Step 5 - Supply chain risks and due diligence part 1

        public class RisksModel
        {
            public RisksModel(short id, short? parentId, string description, string category, string details, bool isSelected)
            {
                Id = id;
                ParentId = parentId;
                Description = description;
                Category = category;
                Details = details;
                IsSelected = isSelected;
            }

            public short Id { get; set; }
            public short? ParentId { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
            public string Category { get; set; }
            public string Details { get; set; }
        }

        public List<RisksModel> RelevantRisks { get; set; }

        public string OtherRelevantRisks { get; set; }

        public List<RisksModel> HighRisks { get; set; }

        public string OtherHighRisks { get; set; }

        public List<RisksModel> LocationRisks { get; set; }

        #endregion

        #region Step 5 - Supply chain risks and due diligence part 2
        public class DiligenceModel
        {
            public DiligenceModel(short id, short? parentId, string description, string details, bool isSelected)
            {
                Id = id;
                ParentId = parentId;
                Description = description;
                Details = details;
                IsSelected = IsSelected;
            }

            public short Id { get; set; }
            public short? ParentId { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
            public string Details { get; set; }
        }

        public List<DiligenceModel> DueDiligences { get; set; }

        public string ForcedLabourDetails { get; set; }

        public string SlaveryInstanceDetails { get; set; }

        public string SlaveryInstanceRemediation { get; set; }

        #endregion

        #region Step 6 - Training
        public class TrainingModel
        {
            public TrainingModel(short statementTrainingTypeId, string description, bool isSelected)
            {
                Id = statementTrainingTypeId;
                Description = description;
                IsSelected = isSelected;
            }

            public short Id { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
        }

        public List<TrainingModel> Training { get; set; }

        public string OtherTraining { get; set; }

        #endregion

        #region Step 7 - Monitoring progress

        public bool? IncludesMeasuringProgress { get; set; }

        public string ProgressMeasures { get; set; }

        public string KeyAchievements { get; set; }

        public YearRanges StatementYears { get; set; }

        #endregion

        public virtual bool IsEmpty()
        {
            //TODO
            return false;
        }

    }
}
