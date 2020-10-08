using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.Core.Entities
{
    partial class Statement
    {
        #region Overrides
        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            var target = obj as Statement;
            if (target == null) return false;

            return StatementId == target.StatementId;
        }

        public override int GetHashCode()
        {
            return StatementId.GetHashCode();
        }
        #endregion

        public const int MinComplienceTurnover = 30000000;
        [NotMapped]
        public string ApprovingPerson
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ApproverLastName)) return null;

                return $"{ApproverFirstName} {ApproverLastName} ({ApproverJobTitle})";
            }
        }
        public void SetStatus(StatementStatuses status, long byUserId, string details = null)
        {
            if (status == Status && details == StatusDetails) return;

            Statuses.Add(
                new StatementStatus
                {
                    StatementId = StatementId,
                    Status = status,
                    StatusDate = VirtualDateTime.Now,
                    StatusDetails = details,
                    ByUserId = byUserId
                });
            Status = status;
            StatusDate = VirtualDateTime.Now;
            StatusDetails = details;
        }

        public StatementYears GetStatementYears()
        {
            return Enums.GetEnumFromRange<StatementYears>(MinStatementYears, MaxStatementYears == null ? 0 : MaxStatementYears.Value);
        }

        public StatementTurnovers GetStatementTurnover()
        {
            return Enums.GetEnumFromRange<StatementTurnovers>(MinTurnover, MaxTurnover == null ? 0 : MaxTurnover.Value);
        }

        public ScopeStatuses GetScopeStatus()
        {
            return Organisation.GetActiveScopeStatus(SubmissionDeadline);
        }
        public string GetReportingPeriod()
        {
            return $"{SubmissionDeadline.AddYears(-1).ToString("yyyy")}/{SubmissionDeadline.ToString("yy")}";
        }

        public bool GetIsLateSubmission()
        {
            return Modified > SubmissionDeadline
                   && this.MinTurnover >= MinComplienceTurnover
                   && GetScopeStatus().IsAny(ScopeStatuses.InScope, ScopeStatuses.PresumedInScope);
        }

        public bool IsVoluntarySubmission()
        {
            return StatementId > 0
                   && this.MinTurnover >= MinComplienceTurnover
                   && GetScopeStatus().IsAny(ScopeStatuses.OutOfScope, ScopeStatuses.PresumedOutOfScope);
        }

        public bool CanBeEdited
            // a stub for checking if this entity is allowed to be edited
            // eg checking state
            => true;

        public bool IsValid()
        {
            // Do we validate here?
            // The validation should run against the entity
            // But what about the ViewModel? There will most likely be overlap
            // Are data annotations enough for the view model?

            //if (Status == StatementStatuses.Draft)
            //{
            //    // fields can be null
            //}
            //else
            //{
            //    // fields are not allowed to be null
            //}

            throw new NotImplementedException();
        }
    }
}
