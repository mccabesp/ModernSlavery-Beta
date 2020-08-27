using System;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Shared.Models;

namespace ModernSlavery.WebUI.Submission.Models
{
    [Serializable]
    public class StatementInfoViewModel
    {
        public readonly StatementInfoModel StatementInfo;
        public readonly string OrganisationIdentifier;
        private readonly IUrlHelper _urlHelper;
        public StatementInfoViewModel(StatementInfoModel statementInfoModel, IUrlHelper urlHelper, string organisationIdentifier)
        {
            StatementInfo = statementInfoModel;
            _urlHelper = urlHelper;
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
                    return "Start draft";
                if (!SubmissionAvailable && DraftAvailable)
                    return "Continue";
                if (SubmissionAvailable && !DraftAvailable)
                    return "Edit and republish";
                if (SubmissionAvailable && DraftAvailable)
                    return "Continue";
                return null;
            }
        }

        public string EditUrl
        {
            get
            {
                if (!SubmissionAvailable && !DraftAvailable) return _urlHelper.Action("BeforeYouStart", "Statement", new { organisationIdentifier = OrganisationIdentifier, year = StatementInfo.ReportingDeadline.Year });
                return _urlHelper.Action("ReviewAndEdit", "Statement", new { organisationIdentifier = OrganisationIdentifier, year = StatementInfo.ReportingDeadline.Year });
            }
        }
    }
}