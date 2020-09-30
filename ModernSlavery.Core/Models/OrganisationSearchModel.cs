using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using AutoMapper;
using ModernSlavery.Core.Classes.StatementTypeIndexes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;

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
        public class KeyName
        {
            #region Automapper
            public class AutoMapperProfile : Profile
            {
                public AutoMapperProfile() : base()
                {
                    CreateMap<OrganisationSearchModel.KeyName, SectorTypes?>().ConstructUsing(s => s == null ? (SectorTypes?)null : (SectorTypes)s.Key);
                    CreateMap<OrganisationSearchModel.KeyName, StatementTurnovers?>().ConstructUsing(s => s == null ? (StatementTurnovers?)null : (StatementTurnovers)s.Key);
                    CreateMap<OrganisationSearchModel.KeyName, StatementYears?>().ConstructUsing(s => s == null ? (StatementYears?)null : (StatementYears)s.Key);

                    CreateMap<OrganisationSearchModel.KeyName, SectorTypeIndex.SectorType>().ConvertUsing<SectorConverter>();
                    CreateMap<OrganisationSearchModel.KeyName, PolicyTypeIndex.PolicyType>().ConvertUsing<PolicyConverter>();
                    CreateMap<OrganisationSearchModel.KeyName, RiskTypeIndex.RiskType>().ConvertUsing<RiskConverter>();
                    CreateMap<OrganisationSearchModel.KeyName, DiligenceTypeIndex.DiligenceType>().ConvertUsing<DiligenceConverter>();
                    CreateMap<OrganisationSearchModel.KeyName, TrainingTypeIndex.TrainingType>().ConvertUsing<TrainingConverter>();
                }

                public class SectorConverter : ITypeConverter<OrganisationSearchModel.KeyName, SectorTypeIndex.SectorType>
                {
                    private readonly SectorTypeIndex _sectorTypeIndex;

                    public SectorConverter(SectorTypeIndex sectorTypeIndex)
                    {
                        _sectorTypeIndex = sectorTypeIndex;
                    }

                    public SectorTypeIndex.SectorType Convert(OrganisationSearchModel.KeyName source, SectorTypeIndex.SectorType destination, ResolutionContext context)
                    {
                        return _sectorTypeIndex.FirstOrDefault(sectorType => sectorType.Id == source.Key);
                    }
                }

                public class PolicyConverter : ITypeConverter<OrganisationSearchModel.KeyName, PolicyTypeIndex.PolicyType>
                {
                    private readonly PolicyTypeIndex _policyTypeIndex;

                    public PolicyConverter(PolicyTypeIndex policyTypeIndex)
                    {
                        _policyTypeIndex = policyTypeIndex;
                    }

                    public PolicyTypeIndex.PolicyType Convert(OrganisationSearchModel.KeyName source, PolicyTypeIndex.PolicyType destination, ResolutionContext context)
                    {
                        return _policyTypeIndex.FirstOrDefault(policyType => policyType.Id == source.Key);
                    }
                }

                public class RiskConverter : ITypeConverter<OrganisationSearchModel.KeyName, RiskTypeIndex.RiskType>
                {
                    private readonly RiskTypeIndex _riskTypeIndex;

                    public RiskConverter(RiskTypeIndex riskTypeIndex)
                    {
                        _riskTypeIndex = riskTypeIndex;
                    }

                    public RiskTypeIndex.RiskType Convert(OrganisationSearchModel.KeyName source, RiskTypeIndex.RiskType destination, ResolutionContext context)
                    {
                        var type = _riskTypeIndex.FirstOrDefault(riskType => riskType.Id == source.Key);
                        type.Description = source.Name;
                        return type;
                    }
                }

                public class DiligenceConverter : ITypeConverter<OrganisationSearchModel.KeyName, DiligenceTypeIndex.DiligenceType>
                {
                    private readonly DiligenceTypeIndex _diligenceTypeIndex;

                    public DiligenceConverter(DiligenceTypeIndex diligenceTypeIndex)
                    {
                        _diligenceTypeIndex = diligenceTypeIndex;
                    }

                    public DiligenceTypeIndex.DiligenceType Convert(OrganisationSearchModel.KeyName source, DiligenceTypeIndex.DiligenceType destination, ResolutionContext context)
                    {
                        var type = _diligenceTypeIndex.FirstOrDefault(diligenceType => diligenceType.Id == source.Key);
                        type.Description = source.Name;
                        return type;
                    }
                }

                public class TrainingConverter : ITypeConverter<OrganisationSearchModel.KeyName, TrainingTypeIndex.TrainingType>
                {
                    private readonly TrainingTypeIndex _trainingTypeIndex;

                    public TrainingConverter(TrainingTypeIndex trainingTypeIndex)
                    {
                        _trainingTypeIndex = trainingTypeIndex;
                    }

                    public TrainingTypeIndex.TrainingType Convert(OrganisationSearchModel.KeyName source, TrainingTypeIndex.TrainingType destination, ResolutionContext context)
                    {
                        var type = _trainingTypeIndex.FirstOrDefault(trainingType => trainingType.Id == source.Key);
                        type.Description = source.Name;
                        return type;
                    }
                }
            }
            #endregion

            public int Key { get; set; }
            public string Name { get; set; }
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

        #region General Properties
        public long? StatementId { get; set; }
        public long ParentOrganisationId { get; set; }
        public int? SubmissionDeadlineYear { get; set; }
        public string OrganisationName { get; set; }

        public KeyName SectorType { get; set; }

        public AddressModel Address { get; set; }

        public string CompanyNumber { get; set; }
        public DateTime Modified { get; set; } = VirtualDateTime.Now;

        #endregion

        #region Group Submission
        public bool GroupSubmission { get; set; }
        public string ParentName { get; set; }
        public long? ChildOrganisationId { get; set; }
        public long? ChildStatementOrganisationId { get; set; }
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
        public List<KeyName> Sectors { get; set; } = new List<KeyName>();
        public string OtherSector { get; set; }

        public KeyName Turnover { get; set; }
        #endregion

        #region Policies
        public List<KeyName> Policies { get; set; } = new List<KeyName>();
        public string OtherPolicies { get; set; }

        #endregion

        #region Supply Chain Risks
        public List<KeyName> RelevantRisks { get; set; } = new List<KeyName>();
        public string OtherRelevantRisks { get; set; }
        public List<KeyName> HighRisks { get; set; } = new List<KeyName>();
        public string OtherHighRisks { get; set; }

        public List<KeyName> LocationRisks { get; set; } = new List<KeyName>();
        #endregion

        #region Due Diligence
        public List<KeyName> DueDiligences { get; set; } = new List<KeyName>();
        public string ForcedLabourDetails { get; set; }
        public string SlaveryInstanceDetails { get; set; }
        public List<string> RemediationTypes { get; set; } = new List<string>();
        #endregion

        #region Training
        public List<KeyName> Training { get; set; } = new List<KeyName>();
        public string OtherTraining { get; set; }
        #endregion

        #region Monitoring progress
        public bool? IncludesMeasuringProgress { get; set; }
        public string ProgressMeasures { get; set; }
        public string KeyAchievements { get; set; }
        public KeyName StatementYears { get; set; }
        #endregion

        public OrganisationSearchModel SetSearchDocumentKey()
        {
            var key = ParentOrganisationId.ToString();
            if (SubmissionDeadlineYear.HasValue) key += $"-{SubmissionDeadlineYear}";
            if (!string.IsNullOrWhiteSpace(ParentName))
            {
                if (ChildOrganisationId.HasValue)
                    key += $"-{ChildOrganisationId}";
                else
                    key += $"-{OrganisationName.ToLower()}";
            }
            SearchDocumentKey = key;
            return this;
        }
    }
}