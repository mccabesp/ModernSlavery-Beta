using System;
using System.Collections.Generic;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.BusinessDomain.Shared.Models
{
    /// <summary>
    /// </summary>
    [Serializable]
    public class StatementInfoModel
    {
        public DateTime ReportingDeadline { get; set; }

        public DateTime? SubmittedStatementModifiedDate { get; set; }

        public bool IsStatementEditable { get; set;  }

        public ScopeStatuses ScopeStatus { get; set; }

        public bool RequiredToReport => ScopeStatus != ScopeStatuses.OutOfScope && ScopeStatus != ScopeStatuses.PresumedOutOfScope;

        public DateTime? DraftStatementModifiedDate { get; set; }
        public bool DraftStatementIsEmpty { get; set; }

        public List<string> GroupSubmissionInfo { get; set; }

    }
}