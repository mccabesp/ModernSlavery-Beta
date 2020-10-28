using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using AutoMapper;
using ModernSlavery.Core.Classes.StatementTypeIndexes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using static ModernSlavery.Core.Entities.Statement;
using static ModernSlavery.Core.Entities.StatementSummary.IStatementSummary1;
using static ModernSlavery.Core.Entities.StatementSummary.IStatementSummary1.StatementRisk;

namespace ModernSlavery.Core.Models
{
    [Serializable]
    public class OrganisationSearchModel
    {
        #region Automapper
        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile() : base()
            {
                CreateMap<OrganisationSearchModel, OrganisationSearchModel>();
            }
        }
        #endregion

        [Serializable]
        public class SummarySearchModel
        {
            #region Policies Fields
            public List<KeyName> Policies { get; set; } = new List<KeyName>();

            public string OtherPolicies { get; set; }
            #endregion

            #region Training Fields
            public List<KeyName> TrainingTargets { get; set; } = new List<KeyName>();

            public string OtherTrainingTargets { get; set; }

            #endregion

            #region Partner Fields

            public List<KeyName> Partners { get; set; } = new List<KeyName>();

            public string OtherPartners { get; set; }
            #endregion

            #region Social Audit Fields
            public List<KeyName> SocialAudits { get; set; } = new List<KeyName>();

            public string OtherSocialAudits { get; set; }
            #endregion

            #region Grievance Mechanism Fields
            public List<KeyName> GrievanceMechanisms { get; set; } = new List<KeyName>();

            public string OtherGrievanceMechanisms { get; set; }
            #endregion

            #region Other Work Conditions Monitoring Fields
            public string OtherWorkConditionsMonitoring { get; set; }
            #endregion

            #region Risks
            [Serializable]
            public class StatementRisk
            {
                public byte Index { get; set; }
                public string Details { get; set; }

                #region Risk Source Fields
                public KeyName LikelySource { get; set; }

                public string OtherLikelySource { get; set; }
                #endregion

                #region Risk Target Fields

                public List<KeyName> Targets { get; set; } = new List<KeyName>();

                public string OtherTargets { get; set; }
                #endregion

                #region Risk Location Fields

                public List<KeyName> Countries { get; set; } = new List<KeyName>();
                #endregion

                #region Overrides
                public override bool Equals(object obj)
                {
                    // Check for null values and compare run-time types.
                    var target = obj as StatementRisk;
                    if (target == null) return false;

                    return Index == target.Index;
                }

                public override int GetHashCode()
                {
                    return Index.GetHashCode();
                }
                #endregion
            }

            public List<KeyName> Risks { get; set; } = new List<KeyName>();

            #endregion

            #region Forced Labour Fields
            public List<KeyName> Indicators { get; set; } = new List<KeyName>();

            public string OtherIndicators { get; set; }
            #endregion

            #region Remediation Fields
            public List<KeyName> Remediations { get; set; } = new List<KeyName>();

            public string OtherRemedations { get; set; }
            #endregion

            #region Progress Measuring Fields
            public string ProgressMeasures { get; set; }
            #endregion
        }

        [Serializable]
        public class KeyName
        {
            #region Automapper
            public class AutoMapperProfile : Profile
            {
                public AutoMapperProfile() : base()
                {
                    CreateMap<KeyName, SectorTypes?>().ConstructUsing(s => s == null ? (SectorTypes?)null : (SectorTypes)s.Key);
                    CreateMap<KeyName, StatementTurnoverRanges?>().ConstructUsing(s => s == null ? (StatementTurnoverRanges?)null : (StatementTurnoverRanges)s.Key);
                    CreateMap<KeyName, StatementYearRanges?>().ConstructUsing(s => s == null ? (StatementYearRanges?)null : (StatementYearRanges)s.Key);
                    CreateMap<KeyName, SectorTypeIndex.SectorType>().ConvertUsing<SectorConverter>();

                    CreateMap<KeyName, PolicyTypes?>().ConstructUsing(s => s == null ? (PolicyTypes?)null : (PolicyTypes)s.Key);
                    CreateMap<KeyName, TrainingTargetTypes?>().ConstructUsing(s => s == null ? (TrainingTargetTypes?)null : (TrainingTargetTypes)s.Key);
                    CreateMap<KeyName, PartnerTypes?>().ConstructUsing(s => s == null ? (PartnerTypes?)null : (PartnerTypes)s.Key);
                    CreateMap<KeyName, SocialAuditTypes?>().ConstructUsing(s => s == null ? (SocialAuditTypes?)null : (SocialAuditTypes)s.Key);
                    CreateMap<KeyName, GrievanceMechanismTypes?>().ConstructUsing(s => s == null ? (GrievanceMechanismTypes?)null : (GrievanceMechanismTypes)s.Key);
                    CreateMap<KeyName, RiskSourceTypes?>().ConstructUsing(s => s == null ? (RiskSourceTypes?)null : (RiskSourceTypes)s.Key);
                    CreateMap<KeyName, RiskTargetTypes?>().ConstructUsing(s => s == null ? (RiskTargetTypes?)null : (RiskTargetTypes)s.Key);
                    CreateMap<KeyName, CountryTypes?>().ConstructUsing(s => s == null ? (CountryTypes?)null : (CountryTypes)s.Key);
                    CreateMap<KeyName, IndicatorTypes?>().ConstructUsing(s => s == null ? (IndicatorTypes?)null : (IndicatorTypes)s.Key);
                    CreateMap<KeyName, RemediationTypes?>().ConstructUsing(s => s == null ? (RemediationTypes?)null : (RemediationTypes)s.Key);

                    CreateMap<SectorTypes,KeyName>().ConstructUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<StatementTurnoverRanges,KeyName>().ConstructUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<StatementYearRanges, KeyName>().ConstructUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<SectorTypeIndex.SectorType,KeyName>().ConvertUsing(s => new KeyName { Key = (int)s.Id, Name = s.Description }); 

                    CreateMap<PolicyTypes,KeyName>().ConstructUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<TrainingTargetTypes, KeyName>().ConstructUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<PartnerTypes,KeyName>().ConstructUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<SocialAuditTypes,KeyName>().ConstructUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<GrievanceMechanismTypes,KeyName>().ConstructUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<RiskSourceTypes,KeyName>().ConstructUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<RiskTargetTypes,KeyName>().ConstructUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<CountryTypes,KeyName>().ConstructUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<IndicatorTypes,KeyName>().ConstructUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<RemediationTypes,KeyName>().ConstructUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                }

                public class SectorConverter : ITypeConverter<KeyName, SectorTypeIndex.SectorType>
                {
                    private readonly SectorTypeIndex _sectorTypeIndex;

                    public SectorConverter(SectorTypeIndex sectorTypeIndex)
                    {
                        _sectorTypeIndex = sectorTypeIndex;
                    }

                    public SectorTypeIndex.SectorType Convert(KeyName source, SectorTypeIndex.SectorType destination, ResolutionContext context)
                    {
                        return _sectorTypeIndex.FirstOrDefault(sectorType => sectorType.Id == source.Key);
                    }
                }
            }
            #endregion

            public int Key { get; set; }
            public string Name { get; set; }

            public override bool Equals(object obj)
            {
                var target = obj as KeyName;
                return target != null && target.Key == Key;
            }

            public override int GetHashCode()
            {
                return Key.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            var target = obj as OrganisationSearchModel;
            return target != null && target.SearchDocumentKey == SearchDocumentKey;
        }

        public override int GetHashCode()
        {
            return SearchDocumentKey.GetHashCode();
        }

        #region Search Properties
        public string SearchDocumentKey { get; set; }
        public string PartialNameForSuffixSearches { get; set; }
        public string PartialNameForCompleteTokenSearches { get; set; }
        public string[] Abbreviations { get; set; }
        public DateTime Timestamp { get; } = VirtualDateTime.Now;

        #endregion

        #region Statement Key Fields
        public long? StatementId { get; set; }
        public long ParentOrganisationId { get; set; }
        public int? SubmissionDeadlineYear { get; set; }
        #endregion

        #region Organisation Fields
        public string OrganisationName { get; set; }
        public string CompanyNumber { get; set; }
        public KeyName SectorType { get; set; }
        public AddressModel Address { get; set; }
        #endregion

        #region Statement Control Fields
        public DateTime Modified { get; set; } = VirtualDateTime.Now;
        #endregion

        #region Group Organisation Fields
        public bool? GroupSubmission { get; set; }
        public string ParentName { get; set; }
        public long? ChildOrganisationId { get; set; }
        public long? ChildStatementOrganisationId { get; set; }
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
        public List<KeyName> Sectors { get; set; } = new List<KeyName>();
        public string OtherSectors { get; set; }
        #endregion

        #region Turnover Fields
        public KeyName Turnover { get; set; }
        #endregion

        #region Statement Years Fields
        public KeyName StatementYears { get; set; }
        #endregion

        #region Statement Summary Fields
        public SummarySearchModel Summary { get; set; }
        #endregion

        #region Methods
        public OrganisationSearchModel SetSearchDocumentKey()
        {
            var key = ParentOrganisationId.ToString();
            if (SubmissionDeadlineYear.HasValue) key += $"-{SubmissionDeadlineYear}";
            if (!string.IsNullOrWhiteSpace(ParentName))
            {
                if (ChildOrganisationId.HasValue)
                    key += $"-{ChildOrganisationId}";
                else
                    key += $"-{OrganisationName.ToLower().GetHashCode()}";
            }
            SearchDocumentKey = key;
            return this;
        }
        #endregion
    }
}