using System;
using ModernSlavery.BusinessDomain.Shared.Models;

namespace ModernSlavery.WebUI.Submission.Models
{
    [Serializable]
    public class StatementInfoViewModel
    {
        public readonly StatementInfoModel StatementInfo;
        public readonly string OrganisationIdentifier;
        public StatementInfoViewModel(StatementInfoModel statementInfoModel, string organisationIdentifier)
        {
            StatementInfo = statementInfoModel;
            OrganisationIdentifier = organisationIdentifier;
        }

        //TODO CanChangeScope needs repopulating
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