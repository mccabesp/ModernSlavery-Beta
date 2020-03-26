namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IViewingService
    {
        ISharedBusinessLogic SharedBusinessLogic { get; }

        IOrganisationBusinessLogic OrganisationBusinessLogic { get; }
        ISearchBusinessLogic SearchBusinessLogic { get; }
        ISubmissionBusinessLogic SubmissionBusinessLogic { get; }
    }
}