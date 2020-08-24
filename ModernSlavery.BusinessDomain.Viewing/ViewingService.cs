using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;

namespace ModernSlavery.BusinessDomain.Viewing
{
    public class ViewingService : IViewingService
    {
        public ISharedBusinessLogic SharedBusinessLogic { get; }
        public IOrganisationBusinessLogic OrganisationBusinessLogic { get; set; }
        public ISearchBusinessLogic SearchBusinessLogic { get; set; }
        public ISubmissionBusinessLogic SubmissionBusinessLogic { get; set; }
        public IStatementBusinessLogic StatementBusinessLogic { get; set; }
        public ViewingService(ISharedBusinessLogic sharedBusinessLogic,
            IOrganisationBusinessLogic organisationBusinessLogic, 
            ISearchBusinessLogic searchBusinessLogic,
            ISubmissionBusinessLogic submissionBusinessLogic,
            IStatementBusinessLogic statementBusinessLogic)
        {
            SharedBusinessLogic = sharedBusinessLogic;
            OrganisationBusinessLogic = organisationBusinessLogic;
            SearchBusinessLogic = searchBusinessLogic;
            SubmissionBusinessLogic = submissionBusinessLogic;
            StatementBusinessLogic = statementBusinessLogic;
        }

    }
}