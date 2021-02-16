using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using AutoMapper;
using ModernSlavery.Core.Classes.StatementTypeIndexes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Entities.StatementSummary.V1;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using static ModernSlavery.Core.Entities.Statement;
using static ModernSlavery.Core.Entities.StatementSummary.V1.IStatementSummary;
using static ModernSlavery.Core.Entities.StatementSummary.V1.StatementSummary;
using static ModernSlavery.Core.Entities.StatementSummary.V1.StatementSummary.StatementRisk;

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
                CreateMap<SummarySearchModel.StatementRisk, StatementRisk>().ReverseMap();

                CreateMap<StatementSummary, SummarySearchModel>();

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
                public string Description { get; set; }

                #region Risk Source Fields
                public KeyName LikelySource { get; set; }

                public string OtherLikelySource { get; set; }

                public List<KeyName> SupplyChainTiers { get; set; } = new List<KeyName>();

                #endregion

                #region Risk Target Fields

                public List<KeyName> Targets { get; set; } = new List<KeyName>();

                public string OtherTargets { get; set; }
                #endregion

                #region Actions or plans field
                public string ActionsOrPlans { get; set; }
                #endregion

                #region Risk Location Fields

                public List<StringKeyName> Countries { get; set; } = new List<StringKeyName>();
                #endregion
            }

            public List<StatementRisk> Risks { get; set; } = new List<StatementRisk>();

            public bool NoRisks { get; set; }

            #endregion

            #region Forced Labour Fields
            public List<KeyName> Indicators { get; set; } = new List<KeyName>();

            public string OtherIndicators { get; set; }
            #endregion

            #region Remediation Fields
            public List<KeyName> Remediations { get; set; } = new List<KeyName>();

            public string OtherRemediations { get; set; }
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
                    // make not nullable!
                    CreateMap<KeyName, SectorTypes?>().ConvertUsing(s => s == null ? (SectorTypes?)null : (SectorTypes)s.Key);
                    CreateMap<KeyName, SectorTypes>().ConvertUsing(s => s == null ? default : (SectorTypes)s.Key);
                    CreateMap<KeyName, StatementTurnoverRanges?>().ConvertUsing(s => s == null ? (StatementTurnoverRanges?)null : (StatementTurnoverRanges)s.Key);
                    CreateMap<KeyName, StatementTurnoverRanges>().ConvertUsing(s => s == null ? default : (StatementTurnoverRanges)s.Key);
                    CreateMap<KeyName, StatementYearRanges?>().ConvertUsing(s => s == null ? (StatementYearRanges?)null : (StatementYearRanges)s.Key);
                    CreateMap<KeyName, StatementYearRanges>().ConvertUsing(s => s == null ? default : (StatementYearRanges)s.Key);
                    CreateMap<KeyName, SectorTypeIndex.SectorType>().ConvertUsing<SectorConverter>();

                    CreateMap<KeyName, PolicyTypes>().ConvertUsing(s => s == null ? PolicyTypes.Unknown : (PolicyTypes)s.Key);
                    CreateMap<KeyName, TrainingTargetTypes>().ConvertUsing(s => s == null ? TrainingTargetTypes.Unknown : (TrainingTargetTypes)s.Key);
                    CreateMap<KeyName, PartnerTypes>().ConvertUsing(s => s == null ? PartnerTypes.Unknown : (PartnerTypes)s.Key);
                    CreateMap<KeyName, SocialAuditTypes>().ConvertUsing(s => s == null ? SocialAuditTypes.Unknown : (SocialAuditTypes)s.Key);
                    CreateMap<KeyName, GrievanceMechanismTypes>().ConvertUsing(s => s == null ? GrievanceMechanismTypes.Unknown : (GrievanceMechanismTypes)s.Key);
                    CreateMap<KeyName, RiskSourceTypes>().ConvertUsing(s => s == null ? RiskSourceTypes.Unknown : (RiskSourceTypes)s.Key);
                    CreateMap<KeyName, SupplyChainTierTypes>().ConvertUsing(s => s == null ? SupplyChainTierTypes.Unknown : (SupplyChainTierTypes)s.Key);
                    CreateMap<KeyName, RiskTargetTypes>().ConvertUsing(s => s == null ? RiskTargetTypes.Unknown : (RiskTargetTypes)s.Key);
                    CreateMap<KeyName, IndicatorTypes>().ConvertUsing(s => s == null ? IndicatorTypes.Unknown : (IndicatorTypes)s.Key);
                    CreateMap<KeyName, RemediationTypes>().ConvertUsing(s => s == null ? RemediationTypes.Unknown : (RemediationTypes)s.Key);

                    CreateMap<SectorTypes, KeyName>().ConvertUsing(s => s == default ? null : new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<StatementTurnoverRanges, KeyName>().ConvertUsing(s => s == default ? null : new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<StatementYearRanges, KeyName>().ConvertUsing(s => s == default ? null : new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<SectorTypeIndex.SectorType, KeyName>().ConvertUsing(s => new KeyName { Key = (int)s.Id, Name = s.Description });

                    CreateMap<PolicyTypes, KeyName>().ConvertUsing(s => s == default ? null : new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<TrainingTargetTypes, KeyName>().ConvertUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<PartnerTypes, KeyName>().ConvertUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<SocialAuditTypes, KeyName>().ConvertUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<GrievanceMechanismTypes, KeyName>().ConvertUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<RiskSourceTypes, KeyName>().ConvertUsing(s => s == default ? null : new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<SupplyChainTierTypes, KeyName>().ConvertUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<RiskTargetTypes, KeyName>().ConvertUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<IndicatorTypes, KeyName>().ConvertUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
                    CreateMap<RemediationTypes, KeyName>().ConvertUsing(s => new KeyName { Key = (int)s, Name = s.GetEnumDescription() });
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

        [Serializable]
        public class StringKeyName
        {
            #region Automapper
            public class AutoMapperProfile : Profile
            {
                public AutoMapperProfile() : base()
                {
                    CreateMap<StringKeyName, GovUkCountry>().ConvertUsing<CountryConverter>();

                    CreateMap<GovUkCountry, StringKeyName>().ConvertUsing(s => new StringKeyName { Key = s.FullReference, Name = s.Name });
                }

                public class CountryConverter : ITypeConverter<StringKeyName, GovUkCountry>
                {
                    readonly IGovUkCountryProvider CountryProvider;

                    public CountryConverter(IGovUkCountryProvider countryProvider)
                    {
                        CountryProvider = countryProvider;
                    }

                    public GovUkCountry Convert(StringKeyName source, GovUkCountry destination, ResolutionContext context)
                    {
                        return CountryProvider.FindByReference(source.Key);
                    }
                }
            }
            #endregion

            public string Key { get; set; }
            public string Name { get; set; }

            public override bool Equals(object obj)
            {
                var target = obj as StringKeyName;
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
        public int? GroupOrganisationCount { get; set; }
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