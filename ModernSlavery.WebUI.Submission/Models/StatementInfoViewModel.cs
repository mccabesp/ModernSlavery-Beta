using System;
using ModernSlavery.BusinessDomain.Shared.Models;

namespace ModernSlavery.WebUI.Submission.Models
{
    [Serializable]
    public class StatementInfoViewModel
    {
        public readonly StatementInfoModel StatementInfo;
        public StatementInfoViewModel(StatementInfoModel statementInfoModel)
        {
            StatementInfo = statementInfoModel;
        }
        public long OrganisationId { get; set; }

        public string OrganisationIdentifier { get; set; }

        public bool CanChangeScope { get; set; }

        public bool SubmissionAvailable => StatementInfo.SubmittedStatementModifiedDate != null;
        public bool DraftAvailable => StatementInfo.DraftStatementModifiedDate!=null && !StatementInfo.DraftStatementIsEmpty;


        public string ButtonText
        {
            get 
            {
                if (!SubmissionAvailable && !DraftAvailable)
                    return "Draft report";
                if (!SubmissionAvailable && DraftAvailable)
                    return "Edit Draft";
                if (SubmissionAvailable && !DraftAvailable)
                    return "Edit Report";
                if (SubmissionAvailable && DraftAvailable)
                    return "Edit Draft Report";
                return null;
            }
        }
    }
}