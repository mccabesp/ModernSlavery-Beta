using AutoMapper;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Entities.StatementSummary;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static ModernSlavery.Core.Entities.Statement;

namespace ModernSlavery.BusinessDomain.Shared.Models
{
    public class StatementModelMapperProfile : Profile
    {
        public StatementModelMapperProfile()
        {
            CreateMap<Statement, StatementModel>()
                .ForMember(d => d.Submitted, opt => opt.MapFrom(s => s.StatementId > 0))
                .ForMember(dest => dest.StatementOrganisations, opt => opt.MapFrom(st => st.StatementOrganisations.Select(s => new StatementModel.StatementOrganisationModel(s))))
                .ForMember(d => d.StatementStartDate, opt => opt.MapFrom(s => s.StatementStartDate.ToNullable()))
                .ForMember(d => d.StatementEndDate, opt => opt.MapFrom(s => s.StatementEndDate.ToNullable()))
                .ForMember(d => d.ApprovedDate, opt => opt.MapFrom(s => s.ApprovedDate.ToNullable()))
                .ForMember(d => d.OrganisationName, opt => opt.MapFrom(s => s.Organisation.OrganisationName))
                .ForMember(dest => dest.Modifications, opt => opt.MapFrom(s => string.IsNullOrWhiteSpace(s.Modifications) ? null : JsonConvert.DeserializeObject<List<AutoMap.Diff>>(s.Modifications)))
                .ForMember(dest => dest.GroupSubmission, opt => opt.MapFrom(s => s.StatementId == 0 ? (bool?)null : s.StatementOrganisations.Any()))
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Sectors, opt => opt.MapFrom(st => st.Sectors.Select(s => s.StatementSectorTypeId)))
                .ForMember(dest => dest.EditorUserId, opt => opt.Ignore())
                .ForMember(dest => dest.EditTimestamp, opt => opt.Ignore())
                .ForMember(dest => dest.DraftBackupDate, opt => opt.Ignore())
                .ForMember(dest => dest.IncludesStructure, opt => opt.MapFrom(st => st.StatementId == 0 ? null : (bool?)st.IncludesStructure))
                .ForMember(dest => dest.IncludesPolicies, opt => opt.MapFrom(st => st.StatementId == 0 ? null : (bool?)st.IncludesPolicies))
                .ForMember(dest => dest.IncludesRisks, opt => opt.MapFrom(st => st.StatementId == 0 ? null : (bool?)st.IncludesRisks))
                .ForMember(dest => dest.IncludesDueDiligence, opt => opt.MapFrom(st => st.StatementId == 0 ? null : (bool?)st.IncludesDueDiligence))
                .ForMember(dest => dest.IncludesTraining, opt => opt.MapFrom(st => st.StatementId == 0 ? null : (bool?)st.IncludesTraining))
                .ForMember(dest => dest.IncludesGoals, opt => opt.MapFrom(st => st.StatementId == 0 ? null : (bool?)st.IncludesGoals));

            CreateMap<StatementModel, Statement>()
                .ForMember(dest => dest.StatementOrganisations, opt => opt.Ignore())
                .ForMember(dest => dest.Sectors, opt => opt.Ignore())
                .ForMember(dest => dest.StatementId, opt => opt.Ignore())
                .ForMember(dest => dest.IsLateSubmission, opt => opt.Ignore())
                .ForMember(dest => dest.Created, opt => opt.Ignore())
                .ForMember(dest => dest.Modified, opt => opt.Ignore())
                .ForMember(dest => dest.Organisation, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.StatusDate, opt => opt.Ignore())
                .ForMember(dest => dest.StatusDetails, opt => opt.Ignore())
                .ForMember(dest => dest.Statuses, opt => opt.Ignore());
        }
    }

    [Serializable]
    public class StatementModel
    {
        #region Draft Key Fields
        public bool Submitted { get; set; }
        public DateTime? DraftBackupDate { get; set; }

        public long EditorUserId { get; set; }
        public DateTime EditTimestamp { get; set; }
        #endregion

        #region Statement Key Fields
        public long? StatementId { get; set; }
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public DateTime SubmissionDeadline { get; set; }
        #endregion

        #region Statement Status Fields
        public bool IsLateSubmission { get; set; }
        public string LateReason { get; set; }

        public StatementStatuses Status { get; set; }
        public DateTime? StatusDate { get; set; }
        #endregion

        #region Statement Control Fields
        [JsonIgnore]
        [IgnoreMap]
        public IList<AutoMap.Diff> Modifications { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
        #endregion

        #region Group Organisation Fields
        public class StatementOrganisationModel
        {
            public StatementOrganisationModel()
            {

            }
            public StatementOrganisationModel(StatementOrganisation statementOrganisation)
            {
                StatementOrganisationId = statementOrganisation.StatementOrganisationId;
                Included = statementOrganisation.Included;
                OrganisationName = statementOrganisation.OrganisationName;
                OrganisationId = statementOrganisation.OrganisationId;
                Address = statementOrganisation.Organisation?.LatestAddress == null ? null : AddressModel.Create(statementOrganisation.Organisation.LatestAddress);
                CompanyNumber = statementOrganisation.Organisation?.CompanyNumber;
                DateOfCessation = statementOrganisation.Organisation?.DateOfCessation;
            }

            public long? StatementOrganisationId { get; set; }
            public bool Included { get; set; }
            public long? OrganisationId { get; set; }
            public string OrganisationName { get; set; }
            public AddressModel Address { get; set; }
            public string CompanyNumber { get; set; }
            public DateTime? DateOfCessation { get; set; }
        }

        public bool? GroupSubmission { get; set; }
        public List<StatementOrganisationModel> StatementOrganisations { get; set; } = new List<StatementOrganisationModel>();
        #endregion

        #region Url & Email Fields
        public string StatementUrl { get; set; }
        public string StatementEmail { get; set; }
        #endregion

        #region Statement Period Fields
        public DateTime? StatementStartDate { get; set; }
        public DateTime? StatementEndDate { get; set; }
        #endregion

        #region Approver & Date Fields
        public string ApproverFirstName { get; set; }
        public string ApproverLastName { get; set; }
        public string ApproverJobTitle { get; set; }
        public DateTime? ApprovedDate { get; set; }
        #endregion

        #region Compliance Fields
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

        #region Sectors Fields
        public SortedSet<short> Sectors { get; set; } = new SortedSet<short>();
        public string OtherSectors { get; set; }
        #endregion

        #region Turnover Fields
        public StatementTurnoverRanges? Turnover;
        #endregion

        #region Statement Years Fields
        public StatementYearRanges? StatementYears { get; set; }
        #endregion

        #region Statement Summary Fields
        public StatementSummary1 Summary { get; set; }
        #endregion

        #region Methods
        public bool IsEmpty()
        {
            return GroupSubmission == null
                && (StatementOrganisations == null || !StatementOrganisations.Any())
                && string.IsNullOrWhiteSpace(StatementUrl)
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
                && (Turnover == null || Turnover == StatementTurnoverRanges.NotProvided)
                && (StatementYears == null || StatementYears == StatementYearRanges.NotProvided)
                && (Summary == null || Summary.IsEmpty());
        }
        #endregion
    }
}
