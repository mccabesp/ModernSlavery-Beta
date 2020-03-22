namespace ModernSlavery.BusinessLogic.Submission
{
    public interface ISubmissionService
    {
        ISharedBusinessLogic SharedBusinessLogic { get; }
        ISubmissionBusinessLogic SubmissionBusinessLogic { get; }
        IScopeBusinessLogic ScopeBusinessLogic { get; }
        IDraftFileBusinessLogic DraftFileBusinessLogic { get; }
    }

    public class SubmissionService : ISubmissionService
    {
        public SubmissionService(
            ISharedBusinessLogic sharedBusinessLogic,
            ISubmissionBusinessLogic submissionBusinessLogic,
            IScopeBusinessLogic scopeBusinessLogic,
            IDraftFileBusinessLogic draftFileBusinessLogic)
        {
            SharedBusinessLogic = sharedBusinessLogic;
            SubmissionBusinessLogic = submissionBusinessLogic;
            ScopeBusinessLogic = scopeBusinessLogic;
            DraftFileBusinessLogic = draftFileBusinessLogic;
        }

        public ISharedBusinessLogic SharedBusinessLogic { get; }
        public ISubmissionBusinessLogic SubmissionBusinessLogic { get; }
        public IScopeBusinessLogic ScopeBusinessLogic { get; }
        public IDraftFileBusinessLogic DraftFileBusinessLogic { get; }
    }
}