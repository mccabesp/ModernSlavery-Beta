using System;
using System.Collections.Generic;
using System.Linq;
using static ModernSlavery.Core.Entities.StatementSummary.IStatementSummary1;

namespace ModernSlavery.Core.Entities.StatementSummary
{
    [Serializable]
    public class StatementSummary1 : IStatementSummary1
    {
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

        public List<StatementRisk> Risks { get; set; } = new List<StatementRisk>();

        #endregion

        #region Forced Labour Fields

        public SortedSet<IndicatorTypes> Indicators { get; set; } = new SortedSet<IndicatorTypes>();

        public string OtherIndicators { get; set; }
        #endregion

        #region Remediation Fields
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
