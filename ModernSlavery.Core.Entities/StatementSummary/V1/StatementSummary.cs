using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace ModernSlavery.Core.Entities.StatementSummary.V1
{
    [Serializable]
    public class StatementSummary : IStatementSummary
    {
        #region Policies Fields
        public enum PolicyTypes
        {
            Unknown = 0,
            [Description("Freedom of workers to terminate employment")] FreedomToTerminate,
            [Description("Freedom of movement")] FreedomOfMovement,
            [Description("Freedom of association")] FreedomOfAssociation,
            [Description("Prohibits any threat of violence, harassment and intimidation")] ViolenceHarassmentIntimidation,
            [Description("Prohibits the use of worker-paid recruitment fees")] RecruitmentFees,
            [Description("Prohibits compulsory overtime")] CompulsoryOvertime,
            [Description("Prohibits child labour")] ChildLabour,
            [Description("Prohibits discrimination")] Discrimination,
            [Description("Prohibits confiscation of workers original identification documents")] DocumentRetention,
            [Description("Provides access to remedy, compensation and justice for victims of modern slavery")] RemedyAccess,
            [Description("Other")] Other,
            [Description("Your organisation's policies do not include any of these provisions in relation to modern slavery")] None
        }

        public SortedSet<PolicyTypes> Policies { get; set; } = new SortedSet<PolicyTypes>();

        public string OtherPolicies { get; set; }
        #endregion

        #region Training Fields
        public enum TrainingTargetTypes
        {
            Unknown,
            [Description("Your whole organisation")] OwnOrganisation,
            [Description("Your front line staff")] OwnStaff,
            [Description("Human resources")] OwnHumanResources,
            [Description("Executive-level staff")] OwnExecutives,
            [Description("Procurement staff")] OwnProcurers,
            [Description("Your suppliers")] Suppliers,
            [Description("The wider community")] WiderCommunity,
            [Description("Other")] Other,
            [Description("Your organisation did not provide training on modern slavery during the period of the statement")] None
        }

        public SortedSet<TrainingTargetTypes> TrainingTargets { get; set; } = new SortedSet<TrainingTargetTypes>();

        public string OtherTrainingTargets { get; set; }

        #endregion

        #region Partner Fields
        public enum PartnerTypes
        {
            Unknown,
            [Description("Your suppliers")] OwnSuppliers,
            [Description("Trade unions or worker representative groups")] TradeUnions,
            [Description("Civil society organisations")] CivilSocietyOrganisations,
            [Description("Professional auditors")] ProfessionalAuditors,
            [Description("Workers within your organisation")] OwnWorkers,
            [Description("Workers within your supply chain")] SupplierWorkers,
            [Description("Central or local government")] Government,
            [Description("Law enforcement, such as police, GLAA and other local labour market inspectorates")] LawEnforcement,
            [Description("Businesses in your industry or sector")] Businesses,
            [Description("Your organisation did not engage with others to help monitor working conditions across your operations and supply chain")] None,
        }

        public SortedSet<PartnerTypes> Partners { get; set; } = new SortedSet<PartnerTypes>();

        public string OtherPartners { get; set; }
        #endregion

        #region Social Audit Fields
        public enum SocialAuditTypes
        {
            Unknown = 0,
            [Description("Audit conducted by your staff")] OwnStaff,
            [Description("Third party audit arranged by your organisation")] ThirdPartyOrganisation,
            [Description("Audit conducted by your supplier’s staff")] SupplierStaff,
            [Description("Third party audit arranged by your supplier")] ThirdPartySupplier,
            [Description("Announced audit")] Announced,
            [Description("Unannounced audit")] Unannounced,
            [Description("Your organisation did not carry out any social audits during the period of the statement")] None,
        }
        public SortedSet<SocialAuditTypes> SocialAudits { get; set; } = new SortedSet<SocialAuditTypes>();

        public string OtherSocialAudits { get; set; }
        #endregion

        #region Grievance Mechanism Fields
        public enum GrievanceMechanismTypes
        {
            Unknown,
            [Description("Using anonymous whistleblowing services, such as a helpline or mobile phone app")] WhistleblowingServices,
            [Description("Through trade unions or other worker representative groups")] WorkerVoicePlatforms,
            [Description("There were no processes in your operations or supply chains for workers to raise concerns or make complaints")] None,
        }
        public SortedSet<GrievanceMechanismTypes> GrievanceMechanisms { get; set; } = new SortedSet<GrievanceMechanismTypes>();

        public string OtherGrievanceMechanisms { get; set; }
        #endregion

        #region Other Work Conditions Monitoring Fields
        public string OtherWorkConditionsMonitoring { get; set; }
        #endregion

        #region Risks
        public class StatementRisk
        {
            public string Description { get; set; }

            #region Risk Source Fields
            public enum RiskSourceTypes
            {
                Unknown,
                [Description("Within your own operations")] OwnOperations,
                [Description("Within your supply chains")] SupplyChains,
                [Description("Other")] Other,
            }

            public RiskSourceTypes LikelySource { get; set; }

            public enum SupplyChainTierTypes
            {
                Unknown,
                [Description("Tier 1 suppliers")] Tier1,
                [Description("Tier 2 suppliers")] Tier2,
                [Description("Tier 3 suppliers and below")] Tier3,
                [Description("Don't know")] None,
            }

            public List<SupplyChainTierTypes> SupplyChainTiers { get; set; } = new List<SupplyChainTierTypes>();

            public string OtherLikelySource { get; set; }
            #endregion

            #region Risk Target Fields
            public enum RiskTargetTypes
            {
                Unknown,
                [Description("Women")] Women,
                [Description("Migrants")] Migrants,
                [Description("Refugees")] Refugees,
                [Description("Children")] Children,
                [Description("Other vulnerable group(s)")] Other,
            }

            public SortedSet<RiskTargetTypes> Targets { get; set; } = new SortedSet<RiskTargetTypes>();

            public string OtherTargets { get; set; }
            #endregion

            #region Actions or Plans Field
            public string ActionsOrPlans { get; set; }
            #endregion

            #region Risk Location Fields
            public SortedSet<GovUkCountry> Countries { get; set; } = new SortedSet<GovUkCountry>();
            #endregion

            #region Methods
            public bool IsEmpty(bool ignoreDescription = false)
            {
                return (ignoreDescription || string.IsNullOrWhiteSpace(Description))
                    && LikelySource == RiskSourceTypes.Unknown && string.IsNullOrWhiteSpace(OtherLikelySource)
                    && (Targets == null || Targets.Count == 0) && string.IsNullOrWhiteSpace(OtherTargets)
                    && (Countries == null || Countries.Count == 0);
            }

            public static string GetTierExplanation(SupplyChainTierTypes tier)
            {
                switch (tier)
                {
                    case SupplyChainTierTypes.Tier1:
                        return "Provide their products and services directly to your organisation.";
                    case SupplyChainTierTypes.Tier2:
                        return "Provide products and services to your organisation via your Tier 1 suppliers.";
                    case SupplyChainTierTypes.Tier3:
                        return "Provide products and services to your organisation via your Tier 2 suppliers or the next higher level in the chain.";
                    default:
                        return string.Empty;
                }
            }
            #endregion
        }

        public List<StatementRisk> Risks { get; set; } = new List<StatementRisk>();
        public bool NoRisks { get; set; }

        #endregion

        #region Forced Labour Fields
        public enum IndicatorTypes
        {
            Unknown,
            [Description("Abuse of vulnerability")] VulnerabilityAbuse,
            [Description("Deception")] Deception,
            [Description("Restriction of movement")] MovementRestriction,
            [Description("Isolation")] Isolation,
            [Description("Physical and sexual violence")] Violence,
            [Description("Intimidation and threats")] ThreatIntimidation,
            [Description("Retention of identity documents")] DocumentRetention,
            [Description("Withholding of wages")] WageWithholding,
            [Description("Debt bondage")] DebtBondage,
            [Description("Abusive working and living conditions")] WorkLiveConditions,
            [Description("Excessive overtime")] ExcessiveOvertime,
            [Description("Other")] Other,
            [Description("My statement does not refer to finding any ILO indicators of forced labour")] None,
        }

        public SortedSet<IndicatorTypes> Indicators { get; set; } = new SortedSet<IndicatorTypes>();

        public string OtherIndicators { get; set; }
        #endregion

        #region Remediation Fields
        public enum RemediationTypes
        {
            Unknown,
            [Description("Financial remediation, including repayment of recruitment fees")] FeeRepayment,
            [Description("Change in policy")] PolicyChange,
            [Description("Change in training")] TrainingChange,
            [Description("Referring potential victims to government services")] VictimReferral,
            [Description("Supporting victims via NGO")] NGOSupport,
            [Description("Supporting investigations by relevant authorities")] CriminalJustice,
            [Description("Other")] Other,
            [Description("My statement does not refer to actions taken in response to finding indicators of forced labour")] None,
        }

        public SortedSet<RemediationTypes> Remediations { get; set; } = new SortedSet<RemediationTypes>();

        public string OtherRemediations { get; set; }
        #endregion

        #region Progress Measuring Fields
        public string ProgressMeasures { get; set; }
        #endregion

        #region Methods
        public bool IsEmpty()
        {
            return Policies.Count == 0 && string.IsNullOrWhiteSpace(OtherPolicies)
                && TrainingTargets.Count == 0 && string.IsNullOrWhiteSpace(OtherTrainingTargets)
                && Partners.Count == 0 && string.IsNullOrWhiteSpace(OtherPartners)
                && SocialAudits.Count == 0 && string.IsNullOrWhiteSpace(OtherSocialAudits)
                && GrievanceMechanisms.Count == 0 && string.IsNullOrWhiteSpace(OtherGrievanceMechanisms)
                && string.IsNullOrWhiteSpace(OtherWorkConditionsMonitoring)
                && (Risks.Count == 0 || Risks.All(r => r.IsEmpty()))
                && Indicators.Count == 0 && string.IsNullOrWhiteSpace(OtherIndicators)
                && Remediations.Count == 0 && string.IsNullOrWhiteSpace(OtherRemediations)
                && string.IsNullOrWhiteSpace(ProgressMeasures);
        }
        #endregion

    }
}
