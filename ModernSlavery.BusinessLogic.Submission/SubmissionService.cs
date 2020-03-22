namespace ModernSlavery.BusinessLogic.Submission
{
    public interface ISubmissionService
    {
        ICommonBusinessLogic CommonBusinessLogic { get; }
        ISubmissionBusinessLogic SubmissionBusinessLogic { get; }
        IScopeBusinessLogic ScopeBusinessLogic { get; }
        IDraftFileBusinessLogic DraftFileBusinessLogic { get; }
    }

    public class SubmissionService : ISubmissionService
    {
        public SubmissionService(
            ICommonBusinessLogic commonBusinessLogic,
            ISubmissionBusinessLogic submissionBusinessLogic,
            IScopeBusinessLogic scopeBusinessLogic,
            IDraftFileBusinessLogic draftFileBusinessLogic)
        {
            CommonBusinessLogic = commonBusinessLogic;
            SubmissionBusinessLogic = submissionBusinessLogic;
            ScopeBusinessLogic = scopeBusinessLogic;
            DraftFileBusinessLogic = draftFileBusinessLogic;
        }

        public ICommonBusinessLogic CommonBusinessLogic { get; }
        public ISubmissionBusinessLogic SubmissionBusinessLogic { get; }
        public IScopeBusinessLogic ScopeBusinessLogic { get; }
        public IDraftFileBusinessLogic DraftFileBusinessLogic { get; }
    }
}