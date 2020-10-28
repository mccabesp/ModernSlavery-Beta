using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Classes.StatementTypeIndexes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Entities.StatementSummary;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using static ModernSlavery.Core.Entities.Statement;
using static ModernSlavery.Core.Entities.StatementSummary.IStatementSummary1;

namespace ModernSlavery.WebUI.Shared.Models
{
    [Serializable()]
    [XmlType("StatementSummary")]
    public class StatementSummaryViewModel: IStatementSummary1
    {
        #region Automapper
        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<OrganisationSearchModel, StatementSummaryViewModel>()
                    .ForMember(d => d.StatementSummaryUrl, opt => opt.Ignore())
                    .ForMember(d => d.BackUrl, opt => opt.Ignore())
                    .ForMember(d => d.Policies, opt => opt.MapFrom(s=>s.Summary.Policies))
                    .ForMember(d => d.OtherPolicies, opt => opt.MapFrom(s=>s.Summary.OtherPolicies))
                    .ForMember(d => d.TrainingTargets, opt => opt.MapFrom(s=>s.Summary.TrainingTargets))
                    .ForMember(d => d.OtherTrainingTargets, opt => opt.MapFrom(s=>s.Summary.OtherTrainingTargets))
                    .ForMember(d => d.Partners, opt => opt.MapFrom(s=>s.Summary.Partners))
                    .ForMember(d => d.OtherPartners, opt => opt.MapFrom(s=>s.Summary.OtherPartners))
                    .ForMember(d => d.SocialAudits, opt => opt.MapFrom(s=>s.Summary.SocialAudits))
                    .ForMember(d => d.OtherSocialAudits, opt => opt.MapFrom(s=>s.Summary.OtherSocialAudits))
                    .ForMember(d => d.GrievanceMechanisms, opt => opt.MapFrom(s=>s.Summary.GrievanceMechanisms))
                    .ForMember(d => d.OtherGrievanceMechanisms, opt => opt.MapFrom(s=>s.Summary.OtherGrievanceMechanisms))
                    .ForMember(d => d.OtherWorkConditionsMonitoring, opt => opt.MapFrom(s=>s.Summary.OtherWorkConditionsMonitoring))
                    .ForMember(d => d.Risks, opt => opt.MapFrom(s=>s.Summary.Risks))
                    .ForMember(d => d.Indicators, opt => opt.MapFrom(s=>s.Summary.Indicators))
                    .ForMember(d => d.OtherIndicators, opt => opt.MapFrom(s=>s.Summary.OtherIndicators))
                    .ForMember(d => d.Remediations, opt => opt.MapFrom(s=>s.Summary.Remediations))
                    .ForMember(d => d.OtherRemedations, opt => opt.MapFrom(s=>s.Summary.OtherRemedations))
                    .ForMember(d => d.ProgressMeasures, opt => opt.MapFrom(s=>s.Summary.ProgressMeasures))
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
        public bool GroupSubmission { get; set; }
        public string ParentName { get; set; }
        public string ChildOrganisationId { get; set; }
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

            public SortedSet<StatementRisk> Risks { get; set; } = new SortedSet<StatementRisk>();

            #endregion

            #region Forced Labour Fields

            public SortedSet<IndicatorTypes> Indicators { get; set; } = new SortedSet<IndicatorTypes>();

            public string OtherIndicators { get; set; }
            #endregion

            #region Remediation Fields
            public SortedSet<RemediationTypes> Remediations { get; set; } = new SortedSet<RemediationTypes>();

            public string OtherRemedations { get; set; }
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