using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static ModernSlavery.Core.Entities.StatementSummary.V1.StatementSummary;

namespace ModernSlavery.Core.Entities.StatementSummary.V1
{
    public interface IStatementSummary
    {
        #region Policies Fields

        SortedSet<PolicyTypes> Policies { get; set; }

        string OtherPolicies { get; set; }
        #endregion

        #region Training Fields


        SortedSet<TrainingTargetTypes> TrainingTargets { get; set; }

        string OtherTrainingTargets { get; set; }

        #endregion

        #region Partner Fields

        SortedSet<PartnerTypes> Partners { get; set; }

        string OtherPartners { get; set; }
        #endregion

        #region Social Audit Fields

        SortedSet<SocialAuditTypes> SocialAudits { get; set; }

        string OtherSocialAudits { get; set; }
        #endregion

        #region Grievance Mechanism Fields

        SortedSet<GrievanceMechanismTypes> GrievanceMechanisms { get; set; }

        string OtherGrievanceMechanisms { get; set; }
        #endregion

        #region Other Work Conditions Monitoring Fields
        string OtherWorkConditionsMonitoring { get; set; }
        #endregion

        #region Risks


        List<StatementRisk> Risks { get; set; }

        #endregion

        #region Forced Labour Fields


        SortedSet<IndicatorTypes> Indicators { get; set; }

        string OtherIndicators { get; set; }
        #endregion

        #region Remediation Fields

        SortedSet<RemediationTypes> Remediations { get; set; }

        string OtherRemediations { get; set; }
        #endregion

        #region Progress Measuring Fields
        string ProgressMeasures { get; set; }
        #endregion

    }
}
