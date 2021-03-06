using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Classes.StatementTypeIndexes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Entities.StatementSummary.V1;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes.UrlHelper;
using static ModernSlavery.Core.Entities.Statement;
using static ModernSlavery.Core.Entities.StatementSummary.V1.StatementSummary;

namespace ModernSlavery.WebUI.Shared.Models
{
    [Serializable()]
    [XmlType("StatementSummary")]
    public class StatementSummaryViewModel: IStatementSummary
    {
        #region Automapper
        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {

                CreateMap<OrganisationSearchModel, StatementSummaryViewModel>()
                    .ForMember(dest => dest.Address, opt => opt.MapFrom(source => source.ChildStatementOrganisationId!=null && source.ChildOrganisationId == null ? null : source.Address))
                    .ForMember(dest => dest.CompanyNumber, opt => opt.MapFrom(source => source.ChildStatementOrganisationId != null && source.ChildOrganisationId == null ? null : source.CompanyNumber))
                    .ForMember(dest => dest.StatementSummaryUrl, opt => opt.Ignore())
                    .ForMember(dest => dest.BackUrl, opt => opt.Ignore())
                    .ForMember(dest => dest.GroupSubmission, opt => opt.MapFrom(s => s.GroupSubmission))
                    .ForMember(dest => dest.Policies, opt => opt.MapFrom(s=>s.Summary.Policies))
                    .ForMember(dest => dest.OtherPolicies, opt => opt.MapFrom(s=>s.Summary.OtherPolicies))
                    .ForMember(dest => dest.TrainingTargets, opt => opt.MapFrom(s=>s.Summary.TrainingTargets))
                    .ForMember(dest => dest.OtherTrainingTargets, opt => opt.MapFrom(s=>s.Summary.OtherTrainingTargets))
                    .ForMember(dest => dest.Partners, opt => opt.MapFrom(s=>s.Summary.Partners))
                    .ForMember(dest => dest.OtherPartners, opt => opt.MapFrom(s=>s.Summary.OtherPartners))
                    .ForMember(dest => dest.SocialAudits, opt => opt.MapFrom(s=>s.Summary.SocialAudits))
                    .ForMember(dest => dest.OtherSocialAudits, opt => opt.MapFrom(s=>s.Summary.OtherSocialAudits))
                    .ForMember(dest => dest.GrievanceMechanisms, opt => opt.MapFrom(s=>s.Summary.GrievanceMechanisms))
                    .ForMember(dest => dest.OtherGrievanceMechanisms, opt => opt.MapFrom(s=>s.Summary.OtherGrievanceMechanisms))
                    .ForMember(dest => dest.OtherWorkConditionsMonitoring, opt => opt.MapFrom(s=>s.Summary.OtherWorkConditionsMonitoring))
                    .ForMember(dest => dest.Risks, opt => opt.MapFrom(s=>s.Summary.Risks))
                    .ForMember(dest => dest.NoRisks, opt => opt.MapFrom(s=>s.Summary.NoRisks))
                    .ForMember(dest => dest.Indicators, opt => opt.MapFrom(s=>s.Summary.Indicators))
                    .ForMember(dest => dest.OtherIndicators, opt => opt.MapFrom(s=>s.Summary.OtherIndicators))
                    .ForMember(dest => dest.Remediations, opt => opt.MapFrom(s=>s.Summary.Remediations))
                    .ForMember(dest => dest.OtherRemediations, opt => opt.MapFrom(s=>s.Summary.OtherRemediations))
                    .ForMember(dest => dest.ProgressMeasures, opt => opt.MapFrom(s=>s.Summary.ProgressMeasures))
                    .AfterMap<ObfuscateAction>();
            }

            public class ObfuscateAction : IMappingAction<OrganisationSearchModel, StatementSummaryViewModel>
            {
                private readonly IUrlHelper _urlHelper;
                private readonly IObfuscator _obfuscator;

                public ObfuscateAction(IUrlHelper urlHelper, IObfuscator obfuscator)
                {
                    _urlHelper = urlHelper;
                    _obfuscator = obfuscator;
                }
                public void Process(OrganisationSearchModel source, StatementSummaryViewModel destination, ResolutionContext context)
                {
                    destination.ParentOrganisationId = _obfuscator.Obfuscate(source.ParentOrganisationId);
                    destination.ChildOrganisationId = source.ChildOrganisationId == null ? null : _obfuscator.Obfuscate(source.ChildOrganisationId.Value);
                    destination.StatementSummaryUrl = _urlHelper.ActionArea("StatementSummary", "Viewing", "Viewing", new { organisationIdentifier = destination.ParentOrganisationId, reportingDeadlineYear = source.SubmissionDeadlineYear }, "https");
                }
            }
        }
        #endregion

        #region Statement Key Fields
        public string ParentOrganisationId { get; set; }
        public int? SubmissionDeadlineYear { get; set; }
        #endregion

        #region Organisation Fields
        public string OrganisationName { get; set; }
        public string CompanyNumber { get; set; }
        public SectorTypes? SectorType { get; set; }
        public AddressModel Address { get; set; }
        #endregion

        #region Statement Control Fields
        public DateTime Modified { get; set; } = VirtualDateTime.Now;
        #endregion

        #region Group Organisation Fields
        public bool? GroupSubmission { get; set; }
        public string ParentName { get; set; }
        public string ChildOrganisationId { get; set; }
        public int? GroupOrganisationCount { get; set; }
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
        public string ApprovingPerson { get; set; }
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
        public List<SectorTypeIndex.SectorType> Sectors { get; set; } = new List<SectorTypeIndex.SectorType>();
        public string OtherSectors { get; set; }
        #endregion

        #region Turnover Fields
        public virtual StatementTurnoverRanges? Turnover { get; set; }
        #endregion

        #region Statement Years Fields
        public StatementYearRanges? StatementYears { get; set; }
        #endregion

        #region Navigation Properties
        public string StatementSummaryUrl { get; set; }

        public string BackUrl { get; set; }

        #endregion

        #region Statement Summary Fields
            #region Policies Fields

            public SortedSet<PolicyTypes> Policies { get; set; } = new SortedSet<PolicyTypes>();

            public string OtherPolicies { get; set; }
            #endregion

            #region Training Fields
            public SortedSet<TrainingTargetTypes> TrainingTargets { get; set; } = new SortedSet<TrainingTargetTypes>();

            public string OtherTrainingTargets { get; set; }

            #endregion

            #region Partner Fields

            public SortedSet<PartnerTypes> Partners { get; set; } = new SortedSet<PartnerTypes>();

            public string OtherPartners { get; set; }
            #endregion

            #region Social Audit Fields

            public SortedSet<SocialAuditTypes> SocialAudits { get; set; } = new SortedSet<SocialAuditTypes>();

            public string OtherSocialAudits { get; set; }
            #endregion

            #region Grievance Mechanism Fields

            public SortedSet<GrievanceMechanismTypes> GrievanceMechanisms { get; set; } = new SortedSet<GrievanceMechanismTypes>();

            public string OtherGrievanceMechanisms { get; set; }
            #endregion

            #region Other Work Conditions Monitoring Fields
            public string OtherWorkConditionsMonitoring { get; set; }
            #endregion

            #region Risks

            public List<StatementRisk> Risks { get; set; } = new List<StatementRisk>();

            public bool NoRisks { get; set; }

            #endregion

            #region Forced Labour Fields

            public SortedSet<IndicatorTypes> Indicators { get; set; } = new SortedSet<IndicatorTypes>();

            public string OtherIndicators { get; set; }
            #endregion

            #region Remediation Fields
            public SortedSet<RemediationTypes> Remediations { get; set; } = new SortedSet<RemediationTypes>();

            public string OtherRemediations { get; set; }
            #endregion

            #region Progress Measuring Fields
            public string ProgressMeasures { get; set; }
            #endregion

        #endregion

        #region Methods
        public bool HasAnyAreaCovered()
        {
            return IncludesStructure.HasValue
                || IncludesPolicies.HasValue
                || IncludesRisks.HasValue
                || IncludesDueDiligence.HasValue
                || IncludesTraining.HasValue
                || IncludesGoals.HasValue;
        }

        public bool HasAllAreasCovered()
        {
            return IncludesStructure.HasValue && IncludesStructure.Value
                && IncludesPolicies.HasValue && IncludesPolicies.Value
                && IncludesRisks.HasValue && IncludesRisks.Value
                && IncludesDueDiligence.HasValue && IncludesDueDiligence.Value
                && IncludesTraining.HasValue && IncludesTraining.Value
                && IncludesGoals.HasValue && IncludesGoals.Value;
        }

        public bool HasNoAreasCovered()
        {
            return IncludesStructure.HasValue && !IncludesStructure.Value
                && IncludesPolicies.HasValue && !IncludesPolicies.Value
                && IncludesRisks.HasValue && !IncludesRisks.Value
                && IncludesDueDiligence.HasValue && !IncludesDueDiligence.Value
                && IncludesTraining.HasValue && !IncludesTraining.Value
                && IncludesGoals.HasValue && !IncludesGoals.Value;
        }
        #endregion
    }
}