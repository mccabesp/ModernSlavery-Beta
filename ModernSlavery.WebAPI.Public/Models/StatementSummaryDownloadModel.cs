using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.UrlHelper;
using ModernSlavery.WebUI.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using static ModernSlavery.Core.Entities.StatementSummary.V1.StatementSummary;
using static ModernSlavery.Core.Entities.StatementSummary.V1.StatementSummary.StatementRisk;

namespace ModernSlavery.WebAPI.Models
{
    [Serializable]
    [XmlType("StatementSummary")]
    public class StatementSummaryDownloadModel
    {
        #region Automapper
        public class StatementSummaryDownloadProfile : Profile
        {
            public StatementSummaryDownloadProfile()
            {
                #region OrganisationSearchModel
                CreateMap<OrganisationSearchModel, StatementSummaryDownloadModel>(MemberList.None)
                    .ForMember(x => x.StatementYear, opt => opt.MapFrom(y => y.SubmissionDeadlineYear))
                    .ForMember(x => x.OrganisationName, opt => opt.MapFrom(y => y.OrganisationName))
                    .ForMember(x => x.Address, opt => opt.MapFrom(y => y.Address==null ? null : y.Address.GetFullAddress(Environment.NewLine)))
                    .ForMember(x => x.SectorType, opt => opt.MapFrom(y => y.SectorType != null ? y.SectorType.Name : null))
                    .ForMember(x => x.CompanyNumber, opt => opt.MapFrom(y => y.CompanyNumber))
                    .ForMember(x => x.LastUpdated, opt => opt.MapFrom(y => y.Modified))

                    .ForMember(x => x.GroupSubmission, opt => opt.MapFrom(y => y.GroupSubmission))
                    .ForMember(x => x.ParentName, opt => opt.MapFrom(y => y.ParentName))

                    .ForMember(x => x.StatementURL, opt => opt.MapFrom(y => y.StatementUrl))
                    .ForMember(x => x.EmailAddressNoURL, opt => opt.MapFrom(y => y.StatementEmail))

                    .ForMember(x => x.StatementStartDate, opt => opt.MapFrom(y => y.StatementStartDate))
                    .ForMember(x => x.StatementEndDate, opt => opt.MapFrom(y => y.StatementEndDate))

                    .ForMember(x => x.ApprovingPerson, opt => opt.MapFrom(y => y.ApprovingPerson))
                    .ForMember(x => x.DateApproved, opt => opt.MapFrom(y => y.ApprovedDate))

                    .ForMember(x => x.StatementIncludesOrgStructure, opt => opt.MapFrom(y => y.IncludesStructure))
                    .ForMember(x => x.NoOrgStructureReason, opt => opt.MapFrom(y => y.StructureDetails))

                    .ForMember(x => x.StatementIncludesPolicies, opt => opt.MapFrom(y => y.IncludesPolicies))
                    .ForMember(x => x.NoPoliciesReason, opt => opt.MapFrom(y => y.PolicyDetails))

                    .ForMember(x => x.StatementIncludesRisksAssessment, opt => opt.MapFrom(y => y.IncludesRisks))
                    .ForMember(x => x.NoRiskAssessmentReason, opt => opt.MapFrom(y => y.RisksDetails))

                    .ForMember(x => x.StatementIncludesDueDiligence, opt => opt.MapFrom(y => y.IncludesDueDiligence))
                    .ForMember(x => x.NoDueDiligenceReason, opt => opt.MapFrom(y => y.DueDiligenceDetails))

                    .ForMember(x => x.StatementIncludesTraining, opt => opt.MapFrom(y => y.IncludesTraining))
                    .ForMember(x => x.NoTrainingReason, opt => opt.MapFrom(y => y.TrainingDetails))

                    .ForMember(x => x.StatementIncludesGoals, opt => opt.MapFrom(y => y.IncludesGoals))
                    .ForMember(x => x.NoGoalsReason, opt => opt.MapFrom(y => y.GoalsDetails))

                    .ForMember(x => x.OrganisationSectors, opt => opt.MapFrom(y => string.Join(Environment.NewLine, y.Sectors.Select(s => s.Name))))
                    .ForMember(x => x.OtherOrganisationSector, opt => opt.MapFrom(y => y.OtherSectors))

                    .ForMember(x => x.Turnover, opt => opt.MapFrom(y => y.Turnover != null ? y.Turnover.Name : null))
                    .ForMember(x => x.YearsProducingStatements, opt => opt.MapFrom(y => y.StatementYears != null ? y.StatementYears.Name : null))

                    .ForMember(x => x.Policies, opt => opt.MapFrom(y => y.Summary.Policies.Select(p => p.Key).Contains((int)PolicyTypes.None) ? null : string.Join(Environment.NewLine, y.Summary.Policies.Select(p => p.Name))))
                    .ForMember(x => x.OtherPolicies, opt => opt.MapFrom(y => y.Summary.OtherPolicies))
                    .ForMember(x => x.NoPolicies, opt => opt.MapFrom(y => !y.Summary.Policies.Any() ? null : (bool?)y.Summary.Policies.Select(p => p.Key).Contains((int)PolicyTypes.None)))

                    .ForMember(x => x.Training, opt => opt.MapFrom(y => y.Summary.TrainingTargets.Select(p => p.Key).Contains((int)TrainingTargetTypes.None) ? null : string.Join(Environment.NewLine, y.Summary.TrainingTargets.Select(p => p.Name))))
                    .ForMember(x => x.OtherTraining, opt => opt.MapFrom(y => y.Summary.OtherTrainingTargets))
                    .ForMember(x => x.NoTraining, opt => opt.MapFrom(y => !y.Summary.TrainingTargets.Any() ? null : (bool?)y.Summary.TrainingTargets.Select(p => p.Key).Contains((int)TrainingTargetTypes.None)))

                    .ForMember(x => x.WorkingConditionsEngagement, opt => opt.MapFrom(y => y.Summary.Partners.Select(p => p.Key).Contains((int)PartnerTypes.None) ? null : string.Join(Environment.NewLine, y.Summary.Partners.Select(p => p.Name))))
                    .ForMember(x => x.NoEngagement, opt => opt.MapFrom(y => !y.Summary.Partners.Any() ? null : (bool?)y.Summary.Partners.Select(p => p.Key).Contains((int)PartnerTypes.None)))

                    .ForMember(x => x.SocialAudits, opt => opt.MapFrom(y => y.Summary.SocialAudits.Select(p => p.Key).Contains((int)SocialAuditTypes.None) ? null : string.Join(Environment.NewLine, y.Summary.SocialAudits.Select(p => p.Name))))
                    .ForMember(x => x.NoSocialAudits, opt => opt.MapFrom(y => !y.Summary.GrievanceMechanisms.Any() ? null : (bool?)y.Summary.SocialAudits.Select(p => p.Key).Contains((int)SocialAuditTypes.None)))

                    .ForMember(x => x.GrievanceMechanisms, opt => opt.MapFrom(y => y.Summary.GrievanceMechanisms.Select(p => p.Key).Contains((int)GrievanceMechanismTypes.None) ? null : string.Join(Environment.NewLine, y.Summary.GrievanceMechanisms.Select(p => p.Name))))
                    .ForMember(x => x.NoGrievanceMechanisms, opt => opt.MapFrom(y => !y.Summary.GrievanceMechanisms.Any() ? null : (bool?)y.Summary.GrievanceMechanisms.Select(p => p.Key).Contains((int)GrievanceMechanismTypes.None)))

                    .ForMember(x => x.OtherMonitoring, opt => opt.MapFrom(y => y.Summary.OtherWorkConditionsMonitoring))

                    .ForMember(x => x.NoRisks, opt => opt.MapFrom(y => y.Summary.NoRisks))

                    .ForMember(x => x.Risk1, opt => opt.MapFrom(y => y.Summary.Risks.FirstOrDefault() == null ? null : y.Summary.Risks.FirstOrDefault().Description))
                    .ForMember(x => x.Risk1Area, opt => opt.MapFrom(y => y.Summary.Risks.FirstOrDefault() == null && y.Summary.Risks.FirstOrDefault().LikelySource == null ? null : (y.Summary.Risks.FirstOrDefault().LikelySource.Key == (int)RiskSourceTypes.Other ? y.Summary.Risks.FirstOrDefault().OtherLikelySource : y.Summary.Risks.FirstOrDefault().LikelySource.Name)))
                    .ForMember(x => x.Risk1SupplyChainTier, opt => opt.MapFrom(y => y.Summary.Risks.FirstOrDefault() == null ? null : string.Join(Environment.NewLine, y.Summary.Risks.FirstOrDefault().SupplyChainTiers.Select(t => t.Name))))
                    .ForMember(x => x.Risk1GroupAffected, opt => opt.MapFrom(y => y.Summary.Risks.FirstOrDefault() == null ? null : string.Join(Environment.NewLine, y.Summary.Risks.FirstOrDefault().Targets.Select(t => t.Name))))
                    .ForMember(x => x.Risk1OtherGroupAffected, opt => opt.MapFrom(y => y.Summary.Risks.FirstOrDefault() == null ? null : y.Summary.Risks.FirstOrDefault().OtherTargets))
                    .ForMember(x => x.Risk1Location, opt => opt.MapFrom(y => y.Summary.Risks.FirstOrDefault() == null ? null : string.Join(Environment.NewLine, y.Summary.Risks.FirstOrDefault().Countries.Select(c => c.Name))))
                    .ForMember(x => x.Risk1Mitigation, opt => opt.MapFrom(y => y.Summary.Risks.FirstOrDefault() == null ? null : y.Summary.Risks.FirstOrDefault().ActionsOrPlans))

                    .ForMember(x => x.Risk2, opt => opt.MapFrom(y => y.Summary.Risks.ElementAtOrDefault(1) == null ? null : y.Summary.Risks.ElementAtOrDefault(1).Description))
                    .ForMember(x => x.Risk2Area, opt => opt.MapFrom(y => y.Summary.Risks.ElementAtOrDefault(1) == null && y.Summary.Risks.ElementAtOrDefault(1).LikelySource == null ? null : (y.Summary.Risks.ElementAtOrDefault(1).LikelySource.Key == (int)RiskSourceTypes.Other ? y.Summary.Risks.ElementAtOrDefault(1).OtherLikelySource : y.Summary.Risks.ElementAtOrDefault(1).LikelySource.Name)))
                    .ForMember(x => x.Risk2SupplyChainTier, opt => opt.MapFrom(y => y.Summary.Risks.ElementAtOrDefault(1) == null ? null : string.Join(Environment.NewLine, y.Summary.Risks.ElementAtOrDefault(1).SupplyChainTiers.Select(t => t.Name))))
                    .ForMember(x => x.Risk2GroupAffected, opt => opt.MapFrom(y => y.Summary.Risks.ElementAtOrDefault(1) == null ? null : string.Join(Environment.NewLine, y.Summary.Risks.ElementAtOrDefault(1).Targets.Select(t => t.Name))))
                    .ForMember(x => x.Risk2OtherGroupAffected, opt => opt.MapFrom(y => y.Summary.Risks.ElementAtOrDefault(1) == null ? null : y.Summary.Risks.ElementAtOrDefault(1).OtherTargets))
                    .ForMember(x => x.Risk2Location, opt => opt.MapFrom(y => y.Summary.Risks.ElementAtOrDefault(1) == null ? null : string.Join(Environment.NewLine, y.Summary.Risks.ElementAtOrDefault(1).Countries.Select(c => c.Name))))
                    .ForMember(x => x.Risk2Mitigation, opt => opt.MapFrom(y => y.Summary.Risks.ElementAtOrDefault(1) == null ? null : y.Summary.Risks.ElementAtOrDefault(1).ActionsOrPlans))

                    .ForMember(x => x.Risk3, opt => opt.MapFrom(y => y.Summary.Risks.ElementAtOrDefault(2) == null ? null : y.Summary.Risks.ElementAtOrDefault(2).Description))
                    .ForMember(x => x.Risk3Area, opt => opt.MapFrom(y => y.Summary.Risks.ElementAtOrDefault(2) == null && y.Summary.Risks.ElementAtOrDefault(2).LikelySource == null ? null : (y.Summary.Risks.ElementAtOrDefault(2).LikelySource.Key == (int)RiskSourceTypes.Other ? y.Summary.Risks.ElementAtOrDefault(2).OtherLikelySource : y.Summary.Risks.ElementAtOrDefault(2).LikelySource.Name)))
                    .ForMember(x => x.Risk3SupplyChainTier, opt => opt.MapFrom(y => y.Summary.Risks.ElementAtOrDefault(2) == null ? null : string.Join(Environment.NewLine, y.Summary.Risks.ElementAtOrDefault(2).SupplyChainTiers.Select(t => t.Name))))
                    .ForMember(x => x.Risk3GroupAffected, opt => opt.MapFrom(y => y.Summary.Risks.ElementAtOrDefault(2) == null ? null : string.Join(Environment.NewLine, y.Summary.Risks.ElementAtOrDefault(2).Targets.Select(t => t.Name))))
                    .ForMember(x => x.Risk3OtherGroupAffected, opt => opt.MapFrom(y => y.Summary.Risks.ElementAtOrDefault(2) == null ? null : y.Summary.Risks.ElementAtOrDefault(2).OtherTargets))
                    .ForMember(x => x.Risk3Location, opt => opt.MapFrom(y => y.Summary.Risks.ElementAtOrDefault(2) == null ? null : string.Join(Environment.NewLine, y.Summary.Risks.ElementAtOrDefault(2).Countries.Select(c => c.Name))))
                    .ForMember(x => x.Risk3Mitigation, opt => opt.MapFrom(y => y.Summary.Risks.ElementAtOrDefault(2) == null ? null : y.Summary.Risks.ElementAtOrDefault(2).ActionsOrPlans))

                    .ForMember(x => x.ILOIndicatorsInStatement, opt => opt.MapFrom(y => y.Summary.Indicators.Select(p => p.Key).Contains((int)IndicatorTypes.None) ? null : string.Join(Environment.NewLine, y.Summary.Indicators.Select(i => i.Name))))
                    .ForMember(x => x.NoILOIndicatorsInStatement, opt => opt.MapFrom(y => !y.Summary.Indicators.Any() ? null : (bool?)y.Summary.Indicators.Select(p => p.Key).Contains((int)IndicatorTypes.None)))

                    .ForMember(x => x.ILOIndicatorsActions, opt => opt.MapFrom(y => y.Summary.Remediations.Select(p => p.Key).Contains((int)RemediationTypes.None) ? null : string.Join(Environment.NewLine, y.Summary.Remediations.Select(i => i.Name))))
                    .ForMember(x => x.ILOIndicatorsNoActions, opt => opt.MapFrom(y => !y.Summary.Remediations.Any() ? null : (bool?)y.Summary.Remediations.Select(p => p.Key).Contains((int)RemediationTypes.None)))

                    .ForMember(x => x.DemonstrateProgress, opt => opt.MapFrom(y => y.Summary.ProgressMeasures))

                    .AfterMap<UrlMappingAction>();

                #endregion
            }

            public class UrlMappingAction : IMappingAction<OrganisationSearchModel, StatementSummaryDownloadModel>
            {
                private readonly IUrlHelper _urlHelper;
                private readonly IObfuscator _obfuscator;

                public UrlMappingAction(IUrlHelper urlHelper, IObfuscator obfuscator)
                {
                    _urlHelper = urlHelper;
                    _obfuscator = obfuscator;
                }

                public void Process(OrganisationSearchModel source, StatementSummaryDownloadModel destination, ResolutionContext context)
                {
                    var id = _obfuscator.Obfuscate(source.ParentOrganisationId);
                    var year = source.SubmissionDeadlineYear;
                    var url = _urlHelper.ActionArea("StatementSummary", "Viewing", "Viewing", new { organisationIdentifier = id, year = year }, "https");
                    destination.StatementSummaryURL = url;
                }
            }
        }
        #endregion

        public int? StatementYear { get; set; }

        public string OrganisationName { get; set; }

        public string Address { get; set; }

        public string SectorType { get; set; }

        public string CompanyNumber { get; set; }

        public DateTime LastUpdated { get; set; } = VirtualDateTime.Now;

        public bool GroupSubmission { get; set; }

        public string ParentName { get; set; }

        public string StatementURL { get; set; }

        public string EmailAddressNoURL { get; set; }

        public string StatementSummaryURL { get; set; }

        public DateTime? StatementStartDate { get; set; }
        public DateTime? StatementEndDate { get; set; }

        public string ApprovingPerson { get; set; }
        public DateTime? DateApproved { get; set; }

        public bool? StatementIncludesOrgStructure { get; set; }
        public string NoOrgStructureReason { get; set; }

        public bool? StatementIncludesPolicies { get; set; }
        public string NoPoliciesReason { get; set; }

        public bool? StatementIncludesRisksAssessment { get; set; }
        public string NoRiskAssessmentReason { get; set; }

        public bool? StatementIncludesDueDiligence { get; set; }
        public string NoDueDiligenceReason { get; set; }

        public bool? StatementIncludesTraining { get; set; }
        public string NoTrainingReason { get; set; }

        public bool? StatementIncludesGoals { get; set; }
        public string NoGoalsReason { get; set; }

        public string OrganisationSectors { get; set; }
        public string OtherOrganisationSector { get; set; }

        public string Turnover { get; set; }

        public string YearsProducingStatements { get; set; }

        public string Policies { get; set; }
        public string OtherPolicies { get; set; }
        public bool? NoPolicies { get; set; }

        public string Training { get; set; }
        public string OtherTraining { get; set; }
        public bool? NoTraining { get; set; }

        public string WorkingConditionsEngagement { get; set; }
        public bool? NoEngagement { get; set; }

        public string SocialAudits { get; set; }
        public bool? NoSocialAudits { get; set; }

        public string GrievanceMechanisms { get; set; }
        public bool? NoGrievanceMechanisms { get; set; }

        public string OtherMonitoring { get; set; }

        public bool NoRisks { get; set; }

        #region Risk 1

        public string Risk1 { get; set; }

        public string Risk1Area { get; set; }

        public string Risk1SupplyChainTier { get; set; }

        public string Risk1GroupAffected { get; set; }

        public string Risk1OtherGroupAffected { get; set; }

        public string Risk1Location { get; set; }

        public string Risk1Mitigation { get; set; }

        #endregion

        #region Risk 2

        public string Risk2 { get; set; }

        public string Risk2Area { get; set; }

        public string Risk2SupplyChainTier { get; set; }

        public string Risk2GroupAffected { get; set; }

        public string Risk2OtherGroupAffected { get; set; }

        public string Risk2Location { get; set; }

        public string Risk2Mitigation { get; set; }

        #endregion

        #region Risk 3

        public string Risk3 { get; set; }

        public string Risk3Area { get; set; }

        public string Risk3SupplyChainTier { get; set; }

        public string Risk3GroupAffected { get; set; }

        public string Risk3OtherGroupAffected { get; set; }

        public string Risk3Location { get; set; }

        public string Risk3Mitigation { get; set; }

        #endregion

        public string ILOIndicatorsInStatement { get; set; }
        public bool? NoILOIndicatorsInStatement { get; set; }

        public string ILOIndicatorsActions { get; set; }
        public bool? ILOIndicatorsNoActions { get; set; }

        public string DemonstrateProgress { get; set; }
    }
}
