﻿using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;

namespace ModernSlavery.BusinessDomain.Viewing
{
    public interface IViewingService
    {
        ISharedBusinessLogic SharedBusinessLogic { get; }

        IOrganisationBusinessLogic OrganisationBusinessLogic { get; }
        ISearchBusinessLogic SearchBusinessLogic { get; }
        ISubmissionBusinessLogic SubmissionBusinessLogic { get; }
    }

    public class ViewingService : IViewingService
    {
        public ViewingService(ISharedBusinessLogic sharedBusinessLogic,
            IOrganisationBusinessLogic organisationBusinessLogic, ISearchBusinessLogic searchBusinessLogic,
            ISubmissionBusinessLogic submissionBusinessLogic)
        {
            SharedBusinessLogic = sharedBusinessLogic;
            OrganisationBusinessLogic = organisationBusinessLogic;
            SearchBusinessLogic = searchBusinessLogic;
            SubmissionBusinessLogic = submissionBusinessLogic;
        }

        public ISharedBusinessLogic SharedBusinessLogic { get; }
        public IOrganisationBusinessLogic OrganisationBusinessLogic { get; set; }
        public ISearchBusinessLogic SearchBusinessLogic { get; set; }
        public ISubmissionBusinessLogic SubmissionBusinessLogic { get; set; }
    }
}