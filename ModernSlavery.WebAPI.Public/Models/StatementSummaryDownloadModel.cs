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
                    .ForMember(dest => dest.StatementYear, opt => opt.MapFrom(source => source.SubmissionDeadlineYear))
                    .ForMember(dest => dest.OrganisationName, opt => opt.MapFrom(source => source.OrganisationName))
                    .ForMember(dest => dest.Address, opt => opt.MapFrom(source => (source.ChildStatementOrganisationId != null && source.ChildOrganisationId==null) || source.Address==null ? null : source.Address.GetFullAddress(Environment.NewLine)))
                    .ForMember(dest => dest.SectorType, opt => opt.MapFrom(source => source.SectorType != null ? source.SectorType.Name : null))
                    .ForMember(dest => dest.CompanyNumber, opt => opt.MapFrom(source => source.ChildStatementOrganisationId != null && source.ChildOrganisationId == null ? null : source.CompanyNumber))
                    .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(source => source.Modified))

                    .ForMember(dest => dest.GroupSubmission, opt => opt.MapFrom(source => source.GroupSubmission))
                    .ForMember(dest => dest.ParentName, opt => opt.MapFrom(source => source.ParentName))

                    .ForMember(dest => dest.StatementURL, opt => opt.MapFrom(source => source.StatementUrl))
                    .ForMember(dest => dest.EmailAddressNoURL, opt => opt.MapFrom(source => source.StatementEmail))

                    .ForMember(dest => dest.StatementStartDate, opt => opt.MapFrom(source => source.StatementStartDate))
                    .ForMember(dest => dest.StatementEndDate, opt => opt.MapFrom(source => source.StatementEndDate))

                    .ForMember(dest => dest.ApprovingPerson, opt => opt.MapFrom(source => source.ApprovingPerson))
                    .ForMember(dest => dest.DateApproved, opt => opt.MapFrom(source => source.ApprovedDate))

                    .ForMember(dest => dest.StatementIncludesOrgStructure, opt => opt.MapFrom(source => source.IncludesStructure))
                    .ForMember(dest => dest.NoOrgStructureReason, opt => opt.MapFrom(source => source.StructureDetails))

                    .ForMember(dest => dest.StatementIncludesPolicies, opt => opt.MapFrom(source => source.IncludesPolicies))
                    .ForMember(dest => dest.NoPoliciesReason, opt => opt.MapFrom(source => source.PolicyDetails))

                    .ForMember(dest => dest.StatementIncludesRisksAssessment, opt => opt.MapFrom(source => source.IncludesRisks))
                    .ForMember(dest => dest.NoRiskAssessmentReason, opt => opt.MapFrom(source => source.RisksDetails))

                    .ForMember(dest => dest.StatementIncludesDueDiligence, opt => opt.MapFrom(source => source.IncludesDueDiligence))
                    .ForMember(dest => dest.NoDueDiligenceReason, opt => opt.MapFrom(source => source.DueDiligenceDetails))

                    .ForMember(dest => dest.StatementIncludesTraining, opt => opt.MapFrom(source => source.IncludesTraining))
                    .ForMember(dest => dest.NoTrainingReason, opt => opt.MapFrom(source => source.TrainingDetails))

                    .ForMember(dest => dest.StatementIncludesGoals, opt => opt.MapFrom(source => source.IncludesGoals))
                    .ForMember(dest => dest.NoGoalsReason, opt => opt.MapFrom(source => source.GoalsDetails))

                    .ForMember(dest => dest.OrganisationSectors, opt => opt.MapFrom(source => string.Join(Environment.NewLine, source.Sectors.Select(s => s.Name))))
                    .ForMember(dest => dest.OtherOrganisationSector, opt => opt.MapFrom(source => source.OtherSectors))

                    .ForMember(dest => dest.Turnover, opt => opt.MapFrom(source => source.Turnover != null ? source.Turnover.Name : null))
                    .ForMember(dest => dest.YearsProducingStatements, opt => opt.MapFrom(source => source.StatementYears != null ? source.StatementYears.Name : null))

                    .ForMember(dest => dest.Policies, opt => opt.MapFrom(source => source.Summary.Policies.Select(p => p.Key).Contains((int)PolicyTypes.None) ? null : string.Join(Environment.NewLine, source.Summary.Policies.Select(p => p.Name))))
                    .ForMember(dest => dest.OtherPolicies, opt => opt.MapFrom(source => source.Summary.OtherPolicies))
                    .ForMember(dest => dest.NoPolicies, opt => opt.MapFrom(source => !source.Summary.Policies.Any() ? null : (bool?)source.Summary.Policies.Select(p => p.Key).Contains((int)PolicyTypes.None)))

                    .ForMember(dest => dest.Training, opt => opt.MapFrom(source => source.Summary.TrainingTargets.Select(p => p.Key).Contains((int)TrainingTargetTypes.None) ? null : string.Join(Environment.NewLine, source.Summary.TrainingTargets.Select(p => p.Name))))
                    .ForMember(dest => dest.OtherTraining, opt => opt.MapFrom(source => source.Summary.OtherTrainingTargets))
                    .ForMember(dest => dest.NoTraining, opt => opt.MapFrom(source => !source.Summary.TrainingTargets.Any() ? null : (bool?)source.Summary.TrainingTargets.Select(p => p.Key).Contains((int)TrainingTargetTypes.None)))

                    .ForMember(dest => dest.WorkingConditionsEngagement, opt => opt.MapFrom(source => source.Summary.Partners.Select(p => p.Key).Contains((int)PartnerTypes.None) ? null : string.Join(Environment.NewLine, source.Summary.Partners.Select(p => p.Name))))
                    .ForMember(dest => dest.NoEngagement, opt => opt.MapFrom(source => !source.Summary.Partners.Any() ? null : (bool?)source.Summary.Partners.Select(p => p.Key).Contains((int)PartnerTypes.None)))

                    .ForMember(dest => dest.SocialAudits, opt => opt.MapFrom(source => source.Summary.SocialAudits.Select(p => p.Key).Contains((int)SocialAuditTypes.None) ? null : string.Join(Environment.NewLine, source.Summary.SocialAudits.Select(p => p.Name))))
                    .ForMember(dest => dest.NoSocialAudits, opt => opt.MapFrom(source => !source.Summary.GrievanceMechanisms.Any() ? null : (bool?)source.Summary.SocialAudits.Select(p => p.Key).Contains((int)SocialAuditTypes.None)))

                    .ForMember(dest => dest.GrievanceMechanisms, opt => opt.MapFrom(source => source.Summary.GrievanceMechanisms.Select(p => p.Key).Contains((int)GrievanceMechanismTypes.None) ? null : string.Join(Environment.NewLine, source.Summary.GrievanceMechanisms.Select(p => p.Name))))
                    .ForMember(dest => dest.NoGrievanceMechanisms, opt => opt.MapFrom(source => !source.Summary.GrievanceMechanisms.Any() ? null : (bool?)source.Summary.GrievanceMechanisms.Select(p => p.Key).Contains((int)GrievanceMechanismTypes.None)))

                    .ForMember(dest => dest.OtherMonitoring, opt => opt.MapFrom(source => source.Summary.OtherWorkConditionsMonitoring))

                    .ForMember(dest => dest.NoRisks, opt => opt.MapFrom(source => source.Summary.NoRisks))

                    .ForMember(dest => dest.Risk1, opt => opt.MapFrom(source => source.Summary.Risks.FirstOrDefault() == null ? null : source.Summary.Risks.FirstOrDefault().Description))
                    .ForMember(dest => dest.Risk1Area, opt => opt.MapFrom(source => source.Summary.Risks.FirstOrDefault() == null && source.Summary.Risks.FirstOrDefault().LikelySource == null ? null : (source.Summary.Risks.FirstOrDefault().LikelySource.Key == (int)RiskSourceTypes.Other ? source.Summary.Risks.FirstOrDefault().OtherLikelySource : source.Summary.Risks.FirstOrDefault().LikelySource.Name)))
                    .ForMember(dest => dest.Risk1SupplyChainTier, opt => opt.MapFrom(source => source.Summary.Risks.FirstOrDefault() == null ? null : string.Join(Environment.NewLine, source.Summary.Risks.FirstOrDefault().SupplyChainTiers.Select(t => t.Name))))
                    .ForMember(dest => dest.Risk1GroupAffected, opt => opt.MapFrom(source => source.Summary.Risks.FirstOrDefault() == null ? null : string.Join(Environment.NewLine, source.Summary.Risks.FirstOrDefault().Targets.Select(t => t.Name))))
                    .ForMember(dest => dest.Risk1OtherGroupAffected, opt => opt.MapFrom(source => source.Summary.Risks.FirstOrDefault() == null ? null : source.Summary.Risks.FirstOrDefault().OtherTargets))
                    .ForMember(dest => dest.Risk1Location, opt => opt.MapFrom(source => source.Summary.Risks.FirstOrDefault() == null ? null : string.Join(Environment.NewLine, source.Summary.Risks.FirstOrDefault().Countries.Select(c => c.Name))))
                    .ForMember(dest => dest.Risk1Mitigation, opt => opt.MapFrom(source => source.Summary.Risks.FirstOrDefault() == null ? null : source.Summary.Risks.FirstOrDefault().ActionsOrPlans))

                    .ForMember(dest => dest.Risk2, opt => opt.MapFrom(source => source.Summary.Risks.ElementAtOrDefault(1) == null ? null : source.Summary.Risks.ElementAtOrDefault(1).Description))
                    .ForMember(dest => dest.Risk2Area, opt => opt.MapFrom(source => source.Summary.Risks.ElementAtOrDefault(1) == null && source.Summary.Risks.ElementAtOrDefault(1).LikelySource == null ? null : (source.Summary.Risks.ElementAtOrDefault(1).LikelySource.Key == (int)RiskSourceTypes.Other ? source.Summary.Risks.ElementAtOrDefault(1).OtherLikelySource : source.Summary.Risks.ElementAtOrDefault(1).LikelySource.Name)))
                    .ForMember(dest => dest.Risk2SupplyChainTier, opt => opt.MapFrom(source => source.Summary.Risks.ElementAtOrDefault(1) == null ? null : string.Join(Environment.NewLine, source.Summary.Risks.ElementAtOrDefault(1).SupplyChainTiers.Select(t => t.Name))))
                    .ForMember(dest => dest.Risk2GroupAffected, opt => opt.MapFrom(source => source.Summary.Risks.ElementAtOrDefault(1) == null ? null : string.Join(Environment.NewLine, source.Summary.Risks.ElementAtOrDefault(1).Targets.Select(t => t.Name))))
                    .ForMember(dest => dest.Risk2OtherGroupAffected, opt => opt.MapFrom(source => source.Summary.Risks.ElementAtOrDefault(1) == null ? null : source.Summary.Risks.ElementAtOrDefault(1).OtherTargets))
                    .ForMember(dest => dest.Risk2Location, opt => opt.MapFrom(source => source.Summary.Risks.ElementAtOrDefault(1) == null ? null : string.Join(Environment.NewLine, source.Summary.Risks.ElementAtOrDefault(1).Countries.Select(c => c.Name))))
                    .ForMember(dest => dest.Risk2Mitigation, opt => opt.MapFrom(source => source.Summary.Risks.ElementAtOrDefault(1) == null ? null : source.Summary.Risks.ElementAtOrDefault(1).ActionsOrPlans))

                    .ForMember(dest => dest.Risk3, opt => opt.MapFrom(source => source.Summary.Risks.ElementAtOrDefault(2) == null ? null : source.Summary.Risks.ElementAtOrDefault(2).Description))
                    .ForMember(dest => dest.Risk3Area, opt => opt.MapFrom(source => source.Summary.Risks.ElementAtOrDefault(2) == null && source.Summary.Risks.ElementAtOrDefault(2).LikelySource == null ? null : (source.Summary.Risks.ElementAtOrDefault(2).LikelySource.Key == (int)RiskSourceTypes.Other ? source.Summary.Risks.ElementAtOrDefault(2).OtherLikelySource : source.Summary.Risks.ElementAtOrDefault(2).LikelySource.Name)))
                    .ForMember(dest => dest.Risk3SupplyChainTier, opt => opt.MapFrom(source => source.Summary.Risks.ElementAtOrDefault(2) == null ? null : string.Join(Environment.NewLine, source.Summary.Risks.ElementAtOrDefault(2).SupplyChainTiers.Select(t => t.Name))))
                    .ForMember(dest => dest.Risk3GroupAffected, opt => opt.MapFrom(source => source.Summary.Risks.ElementAtOrDefault(2) == null ? null : string.Join(Environment.NewLine, source.Summary.Risks.ElementAtOrDefault(2).Targets.Select(t => t.Name))))
                    .ForMember(dest => dest.Risk3OtherGroupAffected, opt => opt.MapFrom(source => source.Summary.Risks.ElementAtOrDefault(2) == null ? null : source.Summary.Risks.ElementAtOrDefault(2).OtherTargets))
                    .ForMember(dest => dest.Risk3Location, opt => opt.MapFrom(source => source.Summary.Risks.ElementAtOrDefault(2) == null ? null : string.Join(Environment.NewLine, source.Summary.Risks.ElementAtOrDefault(2).Countries.Select(c => c.Name))))
                    .ForMember(dest => dest.Risk3Mitigation, opt => opt.MapFrom(source => source.Summary.Risks.ElementAtOrDefault(2) == null ? null : source.Summary.Risks.ElementAtOrDefault(2).ActionsOrPlans))

                    .ForMember(dest => dest.ILOIndicatorsInStatement, opt => opt.MapFrom(source => source.Summary.Indicators.Select(p => p.Key).Contains((int)IndicatorTypes.None) ? null : string.Join(Environment.NewLine, source.Summary.Indicators.Select(i => i.Name))))
                    .ForMember(dest => dest.NoILOIndicatorsInStatement, opt => opt.MapFrom(source => !source.Summary.Indicators.Any() ? null : (bool?)source.Summary.Indicators.Select(p => p.Key).Contains((int)IndicatorTypes.None)))

                    .ForMember(dest => dest.ILOIndicatorsActions, opt => opt.MapFrom(source => source.Summary.Remediations.Select(p => p.Key).Contains((int)RemediationTypes.None) ? null : string.Join(Environment.NewLine, source.Summary.Remediations.Select(i => i.Name))))
                    .ForMember(dest => dest.ILOIndicatorsNoActions, opt => opt.MapFrom(source => !source.Summary.Remediations.Any() ? null : (bool?)source.Summary.Remediations.Select(p => p.Key).Contains((int)RemediationTypes.None)))

                    .ForMember(dest => dest.DemonstrateProgress, opt => opt.MapFrom(source => source.Summary.ProgressMeasures))

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
