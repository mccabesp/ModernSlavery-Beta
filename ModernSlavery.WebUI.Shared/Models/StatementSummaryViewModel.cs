using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Classes.StatementTypeIndexes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes.Extensions;

namespace ModernSlavery.WebUI.Shared.Models
{
    [Serializable()]
    [XmlType("StatementSummary")]
    public class StatementSummaryViewModel
    {
        #region Automapper
        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<OrganisationSearchModel, StatementSummaryViewModel>()
                    .ForMember(d => d.StatementSummaryUrl, opt => opt.Ignore())
                    .ForMember(d => d.BackUrl, opt => opt.Ignore())
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

        #region General Properties
        public string StatementSummaryUrl { get; set; }
        public string ParentOrganisationId { get; set; }
        public int? SubmissionDeadlineYear { get; set; }
        public string OrganisationName { get; set; }
        public SectorTypes? SectorType { get; set; }

        public AddressModel Address { get; set; }

        public string CompanyNumber { get; set; }
        public DateTime Modified { get; set; } = VirtualDateTime.Now;
        public string BackUrl { get; set; }

        #endregion

        #region Group Submission
        public bool GroupSubmission { get; set; }
        public string ParentName { get; set; }
        public string ChildOrganisationId { get; set; }
        #endregion

        #region Your Statement
        public string StatementUrl { get; set; }
        public DateTime? StatementStartDate { get; set; }
        public DateTime? StatementEndDate { get; set; }
        public string ApprovingPerson { get; set; }
        public DateTime? ApprovedDate { get; set; }
        #endregion

        #region Compliance
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

        #region Your Organisation
        public List<SectorTypeIndex.SectorType> Sectors { get; set; } = new List<SectorTypeIndex.SectorType>();
        public string OtherSector { get; set; }

        public virtual StatementTurnovers? Turnover { get; set; }
        #endregion

        #region Policies

        public List<PolicyTypeIndex.PolicyType> Policies { get; set; } = new List<PolicyTypeIndex.PolicyType>();

        public string OtherPolicies { get; set; }

        #endregion

        #region Supply Chain Risks
        public List<RiskTypeIndex.RiskType> RelevantRisks { get; set; } = new List<RiskTypeIndex.RiskType>();

        public List<short> GetRelevantRiskParentIds()
        {
            var parentIds = RelevantRisks.Where(d => d.ParentId != null).OrderBy(d => d.Description).Select(d => d.ParentId.Value).Distinct().ToList();
            return parentIds;
        }

        public string OtherRelevantRisks { get; set; }
        public List<RiskTypeIndex.RiskType> HighRisks { get; set; } = new List<RiskTypeIndex.RiskType>();
        public List<short> GetHighRiskParentIds()
        {
            var parentIds = HighRisks.Where(d => d.ParentId != null).OrderBy(d => d.Description).Select(d => d.ParentId.Value).Distinct().ToList();
            return parentIds;
        }


        public string OtherHighRisks { get; set; }

        public List<RiskTypeIndex.RiskType> LocationRisks { get; set; } = new List<RiskTypeIndex.RiskType>();

        public List<short> GetLocationRiskParentIds()
        {
            var parentIds = LocationRisks.Where(d => d.ParentId != null).OrderBy(d => d.Description).Select(d => d.ParentId.Value).Distinct().ToList();
            return parentIds;
        }

        #endregion

        #region Due Diligence
        public List<DiligenceTypeIndex.DiligenceType> DueDiligences { get; set; } = new List<DiligenceTypeIndex.DiligenceType>();
        public List<short> GetDiligenceParentIds()
        {
            var parentIds = DueDiligences.Where(d => d.ParentId != null).OrderBy(d => d.Description).Select(d => d.ParentId.Value).Distinct().ToList();
            return parentIds;
        }

        public string ForcedLabourDetails { get; set; }
        public string SlaveryInstanceDetails { get; set; }
        public List<string> RemediationTypes { get; set; } = new List<string>();
        #endregion

        #region Training
        public List<TrainingTypeIndex.TrainingType> Training { get; set; } = new List<TrainingTypeIndex.TrainingType>();
        public string OtherTraining { get; set; }
        #endregion

        #region Monitoring progress
        public bool? IncludesMeasuringProgress { get; set; }
        public string ProgressMeasures { get; set; }
        public string KeyAchievements { get; set; }
        public StatementYears? StatementYears { get; set; }
        #endregion

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
    }
}