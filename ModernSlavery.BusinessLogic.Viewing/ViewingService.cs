using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.BusinessLogic.Viewing
{
    public interface IViewingService
    {
        ISharedBusinessLogic SharedBusinessLogic { get; }
        IOrganisationBusinessLogic OrganisationBusinessLogic { get; set; }
        ISubmissionBusinessLogic SubmissionBusinessLogic { get; set; }
        ISearchBusinessLogic SearchBusinessLogic { get; set; }

        IObfuscator Obfuscator { get; }
    }

    public class ViewingService : IViewingService
    {
        public ViewingService(
            IOrganisationBusinessLogic organisationBusinessLogic,
            ISharedBusinessLogic sharedBusinessLogic,
            ISubmissionBusinessLogic submissionBusinessLogic,
            ISearchBusinessLogic searchBusinessLogic,
            IObfuscator obfuscator)
        {
            OrganisationBusinessLogic = organisationBusinessLogic;
            SharedBusinessLogic = sharedBusinessLogic;
            SubmissionBusinessLogic = submissionBusinessLogic;
            SearchBusinessLogic = searchBusinessLogic;
            Obfuscator = obfuscator;
        }

        public ISharedBusinessLogic SharedBusinessLogic { get; }
        public IOrganisationBusinessLogic OrganisationBusinessLogic { get; set; }
        public ISubmissionBusinessLogic SubmissionBusinessLogic { get; set; }
        public ISearchBusinessLogic SearchBusinessLogic { get; set; }
        public IObfuscator Obfuscator { get; }
    }
}