using System;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.WebUI.Submission.Models
{
    [Serializable]
    public class StatementInfoViewModel
    {
        public readonly StatementInfoModel StatementInfo;
        public readonly string OrganisationIdentifier;
        private readonly IUrlHelper _urlHelper;
        private readonly IReportingDeadlineHelper ReportingDeadlineHelper;

        public StatementInfoViewModel(StatementInfoModel statementInfoModel,
            IUrlHelper urlHelper,
            IReportingDeadlineHelper reportingDeadlineHelper,
            string organisationIdentifier)
        {
            StatementInfo = statementInfoModel;
            _urlHelper = urlHelper;
            ReportingDeadlineHelper = reportingDeadlineHelper;
            OrganisationIdentifier = organisationIdentifier;
        }

        public bool SubmissionAvailable => StatementInfo.SubmittedStatementModifiedDate != null;
        public bool DraftAvailable => StatementInfo.DraftStatementModifiedDate != null && !StatementInfo.DraftStatementIsEmpty;

        public string ButtonText
        {
            get
            {
                if (!SubmissionAvailable && !DraftAvailable)
                    return "Start draft";
                if (!SubmissionAvailable && DraftAvailable)
                    return "Continue";
                if (SubmissionAvailable)
                    return "Edit and republish";
                return null;
            }
        }

        public string EditUrl
        {
            get
            {
                if (!SubmissionAvailable && !DraftAvailable) return _urlHelper.Action("BeforeYouStart", "Statement", new { organisationIdentifier = OrganisationIdentifier, year = StatementInfo.ReportingDeadline.Year });
                return _urlHelper.Action("Review", "Statement", new { organisationIdentifier = OrganisationIdentifier, year = StatementInfo.ReportingDeadline.Year });
            }
        }
    }
}