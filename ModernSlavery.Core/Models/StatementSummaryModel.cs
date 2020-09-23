using System;
using System.Collections.Generic;
using AutoMapper;
using ModernSlavery.Core.Classes.StatementTypeIndexes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Models
{
    [Serializable]
    public class StatementSummaryModel: OrganisationSearchModel
    {
        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<KeyName, SectorTypes?>().ConstructUsing(s => s == null ? (SectorTypes?)null : (SectorTypes)s.Key);
                CreateMap<OrganisationSearchModel, StatementSummaryModel>();
            }
        }

        #region General Properties
        public new SectorTypes? SectorType { get; set; }
        #endregion

        #region Your Organisation
        public List<SectorTypeIndex.SectorType> Sectors { get; set; } = new List<SectorTypeIndex.SectorType>();
        public virtual StatementTurnovers? Turnover { get; set; }
        #endregion

        #region Policies
        public List<PolicyTypeIndex.PolicyType> Policies { get; set; } = new List<PolicyTypeIndex.PolicyType>();
        #endregion

        #region Supply Chain Risks
        public new List<RiskTypeIndex.RiskType> RelevantRisks { get; set; } = new List<RiskTypeIndex.RiskType>();
        public new List<RiskTypeIndex.RiskType> HighRisks { get; set; } = new List<RiskTypeIndex.RiskType>();
        public new List<RiskTypeIndex.RiskType> LocationRisks { get; set; } = new List<RiskTypeIndex.RiskType>();
        #endregion

        #region Due Diligence
        public new List<DiligenceTypeIndex.DiligenceType> DueDiligences { get; set; } = new List<DiligenceTypeIndex.DiligenceType>();
        #endregion

        #region Training
        public new List<TrainingTypeIndex.TrainingType> Training { get; set; } = new List<TrainingTypeIndex.TrainingType>();
        #endregion

        #region Monitoring progress
        public new StatementYears? StatementYears { get; set; }
        #endregion
    }
}