using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using static ModernSlavery.Core.Entities.StatementSummary.V1.StatementSummary;
using static ModernSlavery.Core.Entities.StatementSummary.V1.StatementSummary.StatementRisk;

namespace ModernSlavery.Core.Models.LogModels
{
    [Serializable]
    public class SubmissionLogModel
    {
        private static readonly string SEPARATOR = Environment.NewLine;

        public int StatementYear { get; set; }
        public long StatementId { get; set; }
        public string OrganisationName { get; set; }
        public long OrganisationId { get; set; }
        public string Address { get; set; }
        public string SectorType { get; set; }
        public string CompanyNumber { get; set; }
        public DateTime Modified { get; set; }
        public string GroupSubmission { get; set; }
        public string ParentName { get; set; }
        public string StatementURL { get; set; }
        public string EmailAddressNoURL { get; set; }
        public string StatementSummaryURL { get; set; }
        public string StatementStartDate { get; set; }
        public string StatementEndDate { get; set; }
        public string ApprovingPerson { get; set; }
        public string DateApproved { get; set; }
        public string StatementIncludesOrgStructure { get; set; }
        public string NoOrgStructureReason { get; set; }
        public string StatementIncludesPolicies { get; set; }
        public string NoPoliciesReason { get; set; }
        public string StatementIncludesRisksAssessment { get; set; }
        public string NoRiskAssessmentReason { get; set; }
        public string StatementIncludesDueDiligence { get; set; }
        public string NoDueDiligenceReason { get; set; }
        public string StatementIncludesTraining { get; set; }
        public string NoTrainingReason { get; set; }
        public string StatementIncludesGoals { get; set; }
        public string NoGoalsReason { get; set; }
        public string OrganisationSectors { get; set; }
        public string OtherOrganisationSector { get; set; }
        public string Turnover { get; set; }
        public string YearsProducingStatements { get; set; }
        public string Policies { get; set; }
        public string OtherPolicies { get; set; }
        public string NoPolicies { get; set; }
        public string Training { get; set; }
        public string OtherTraining { get; set; }
        public string NoTraining { get; set; }
        public string WorkingConditionsEngagement { get; set; }
        public string NoEngagement { get; set; }
        public string SocialAudits { get; set; }
        public string NoSocialAudits { get; set; }
        public string GrievanceMechanisms { get; set; }
        public string NoGrievanceMechanisms { get; set; }
        public string OtherMonitoring { get; set; }
        public string NoRisks { get; set; }
        public string Risk1 { get; set; }
        public string Risk1Area { get; set; }
        public string Risk1SupplyChainTier { get; set; }
        public string Risk1GroupAffected { get; set; }
        public string Risk1OtherGroupAffected { get; set; }
        public string Risk1Location { get; set; }
        public string Risk1Mitigation { get; set; }
        public string Risk2 { get; set; }
        public string Risk2Area { get; set; }
        public string Risk2SupplyChainTier { get; set; }
        public string Risk2GroupAffected { get; set; }
        public string Risk2OtherGroupAffected { get; set; }
        public string Risk2Location { get; set; }
        public string Risk2Mitigation { get; set; }
        public string Risk3 { get; set; }
        public string Risk3Area { get; set; }
        public string Risk3SupplyChainTier { get; set; }
        public string Risk3GroupAffected { get; set; }
        public string Risk3OtherGroupAffected { get; set; }
        public string Risk3Location { get; set; }
        public string Risk3Mitigation { get; set; }
        public string ILOIndicatorsInStatement { get; set; }
        public string NoILOIndicatorsInStatement { get; set; }
        public string ILOIndicatorsActions { get; set; }
        public string ILOIndicatorsNoActions { get; set; }
        public string DemonstrateProgress { get; set; }

        // User fields
        public string UserFirstname { get; set; }
        public string UserLastname { get; set; }
        public string UserJobtitle { get; set; }
        public string UserEmail { get; set; }

        // Contact fields
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string ContactJobTitle { get; set; }
        public string ContactOrganisation { get; set; }
        public string ContactPhoneNumber { get; set; }

        public string IPAddress { get; set; }

        public static SubmissionLogModel Create(Statement statement, string ip, string url)
        {
            var org = statement.Organisation;
            var summary = statement.Summary;
            var status = statement.Statuses.FirstOrDefault(s => s.Status == statement.Status && s.StatusDate == statement.StatusDate);
            var result = new SubmissionLogModel {
                StatementYear = statement.SubmissionDeadline.Year,
                StatementId = statement.StatementId,
                OrganisationName = org.OrganisationName,
                OrganisationId = org.OrganisationId,
                Address = org.LatestAddress?.GetAddressString(SEPARATOR),
                SectorType = org.SectorType.GetEnumDescription(),
                CompanyNumber = org.CompanyNumber,
                Modified = statement.Modified,

                GroupSubmission = Format(statement.StatementOrganisations.Any()),
                ParentName = org.OrganisationName,
                StatementURL = statement.StatementUrl,
                EmailAddressNoURL = statement.StatementEmail,

                StatementSummaryURL = url,

                StatementStartDate = statement.StatementStartDate.ToShortDateString(),
                StatementEndDate = statement.StatementEndDate.ToShortDateString(),
                ApprovingPerson = statement.ApprovingPerson,
                DateApproved = statement.ApprovedDate.ToShortDateString(),
                StatementIncludesOrgStructure = Format(statement.IncludesStructure),
                NoOrgStructureReason = statement.StructureDetails,
                StatementIncludesPolicies = Format(statement.IncludesPolicies),
                NoPoliciesReason = statement.PolicyDetails,
                StatementIncludesRisksAssessment = Format(statement.IncludesRisks),
                NoRiskAssessmentReason = statement.RisksDetails,
                StatementIncludesDueDiligence = Format(statement.IncludesDueDiligence),
                NoDueDiligenceReason = statement.DueDiligenceDetails,
                StatementIncludesTraining = Format(statement.IncludesTraining),
                NoTrainingReason = statement.TrainingDetails,
                StatementIncludesGoals = Format(statement.IncludesGoals),
                NoGoalsReason = statement.GoalsDetails,
                OrganisationSectors = string.Join(SEPARATOR, statement.Sectors.Select(s => s.StatementSectorType.Description)),
                OtherOrganisationSector = statement.OtherSectors,
                Turnover = statement.Turnover.GetEnumDescription(),
                YearsProducingStatements = statement.StatementYears.GetEnumDescription(),

                Policies = JoinExcluding(summary.Policies, PolicyTypes.None),
                OtherPolicies = summary.OtherPolicies,
                NoPolicies = Format(summary.Policies.Contains(PolicyTypes.None)),
                Training = JoinExcluding(summary.TrainingTargets, TrainingTargetTypes.None),
                OtherTraining = summary.OtherTrainingTargets,
                NoTraining = Format(summary.TrainingTargets.Contains(TrainingTargetTypes.None)),
                WorkingConditionsEngagement = JoinExcluding(summary.Partners, PartnerTypes.None),
                NoEngagement = Format(summary.Partners.Contains(PartnerTypes.None)),
                SocialAudits = JoinExcluding(summary.SocialAudits, SocialAuditTypes.None),
                NoSocialAudits = Format(summary.SocialAudits.Contains(SocialAuditTypes.None)),
                GrievanceMechanisms = JoinExcluding(summary.GrievanceMechanisms, GrievanceMechanismTypes.None),
                NoGrievanceMechanisms = Format(summary.GrievanceMechanisms.Contains(GrievanceMechanismTypes.None)),
                OtherMonitoring = summary.OtherWorkConditionsMonitoring,

                NoRisks = Format(summary.NoRisks),

                ILOIndicatorsInStatement = JoinExcluding(summary.Indicators, IndicatorTypes.None),
                NoILOIndicatorsInStatement = Format(summary.Indicators.Contains(IndicatorTypes.None)),
                ILOIndicatorsActions = JoinExcluding(summary.Remediations, RemediationTypes.None),
                ILOIndicatorsNoActions = Format(summary.Remediations.Contains(RemediationTypes.None)),
                DemonstrateProgress = summary.ProgressMeasures,

                UserFirstname = status.ByUser.Firstname,
                UserLastname = status.ByUser.Lastname,
                UserJobtitle = status.ByUser.JobTitle,
                UserEmail = status.ByUser.EmailAddress,

                ContactFirstName = status.ByUser.ContactFirstName,
                ContactLastName = status.ByUser.ContactLastName,
                ContactJobTitle = status.ByUser.JobTitle,
                ContactOrganisation = status.ByUser.ContactOrganisation,
                ContactPhoneNumber = status.ByUser.ContactPhoneNumber,

                IPAddress = ip
            };

            var risk1 = summary.Risks.ElementAtOrDefault(0);
            if (risk1 != null)
            {
                result.Risk1 = risk1.Description;
                result.Risk1Area = risk1.LikelySource == RiskSourceTypes.Other ? risk1.OtherLikelySource : risk1.LikelySource.GetEnumDescription();
                result.Risk1SupplyChainTier = JoinExcluding(risk1.SupplyChainTiers, SupplyChainTierTypes.None);
                result.Risk1GroupAffected = JoinExcluding(risk1.Targets);
                result.Risk1OtherGroupAffected = risk1.OtherTargets;
                result.Risk1Location = string.Join(SEPARATOR, risk1.Countries.Select(c => c.Name));
                result.Risk1Mitigation = risk1.ActionsOrPlans;
            }
            var risk2 = summary.Risks.ElementAtOrDefault(1);
            if (risk2 != null)
            {
                result.Risk2 = risk2.Description;
                result.Risk2Area = risk2.LikelySource == RiskSourceTypes.Other ? risk2.OtherLikelySource : risk2.LikelySource.GetEnumDescription();
                result.Risk2SupplyChainTier = JoinExcluding(risk2.SupplyChainTiers, SupplyChainTierTypes.None);
                result.Risk2GroupAffected = JoinExcluding(risk2.Targets);
                result.Risk2OtherGroupAffected = risk2.OtherTargets;
                result.Risk2Location = string.Join(SEPARATOR, risk2.Countries.Select(c => c.Name));
                result.Risk2Mitigation = risk2.ActionsOrPlans;
            }
            var risk3 = summary.Risks.ElementAtOrDefault(2);
            if (risk3 != null)
            {
                result.Risk3 = risk3.Description;
                result.Risk3Area = risk3.LikelySource == RiskSourceTypes.Other ? risk3.OtherLikelySource : risk3.LikelySource.GetEnumDescription();
                result.Risk3SupplyChainTier = JoinExcluding(risk3.SupplyChainTiers, SupplyChainTierTypes.None);
                result.Risk3GroupAffected = JoinExcluding(risk3.Targets);
                result.Risk3OtherGroupAffected = risk3.OtherTargets;
                result.Risk3Location = string.Join(SEPARATOR, risk3.Countries.Select(c => c.Name));
                result.Risk3Mitigation = risk3.ActionsOrPlans;
            }

            return result;
        }

        private static string Format(bool value)
            => value ? "Yes" : "No";

        private static string JoinExcluding<T>(IEnumerable<T> source, params T[] exclude)
            where T : Enum
        {
            if (source == null)
                return null;

            var result = source.Where(e => !exclude.Contains(e)).Select(e => e.GetEnumDescription());
            return string.Join(SEPARATOR, result);
        }
    }
}