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
            StatusDate = VirtualDateTime.Now;
            Statuses.Add(
                new StatementStatus
                {
                    StatementId = StatementId,
                    Status = status,
                    StatusDate = StatusDate,
                    StatusDetails = details,
                    ByUserId = byUserId
                });
            Status = status;
            StatusDetails = details;
        }

        public ScopeStatuses GetScopeStatus()
        {
            return Organisation.GetActiveScopeStatus(SubmissionDeadline);
        }

        public bool GetIsLateSubmission()
        {
            return Modified > SubmissionDeadline
                   && this.Turnover > StatementTurnoverRanges.Under36Million
                   && GetScopeStatus().IsAny(ScopeStatuses.InScope, ScopeStatuses.PresumedInScope);
        }

        public bool IsVoluntarySubmission()
        {
            return StatementId > 0
                   && this.Turnover > StatementTurnoverRanges.Under36Million
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
