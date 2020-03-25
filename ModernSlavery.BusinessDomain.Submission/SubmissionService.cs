using Autofac.Features.AttributeFilters;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.BusinessDomain.Submission
{
    public interface ISubmissionService
    {
        ISharedBusinessLogic SharedBusinessLogic { get; }
        ISubmissionBusinessLogic SubmissionBusinessLogic { get; }
        IScopeBusinessLogic ScopeBusinessLogic { get; }
        IDraftFileBusinessLogic DraftFileBusinessLogic { get; }
        IPagedRepository<EmployerRecord> PrivateSectorRepository { get; }
    }

    public class SubmissionService : ISubmissionService
    {
        public SubmissionService(
            ISharedBusinessLogic sharedBusinessLogic,
            ISubmissionBusinessLogic submissionBusinessLogic,
            IScopeBusinessLogic scopeBusinessLogic,
            [KeyFilter("Private")] IPagedRepository<EmployerRecord> privateSectorRepository,
            IDraftFileBusinessLogic draftFileBusinessLogic)
        {
            SharedBusinessLogic = sharedBusinessLogic;
            SubmissionBusinessLogic = submissionBusinessLogic;
            ScopeBusinessLogic = scopeBusinessLogic;
            DraftFileBusinessLogic = draftFileBusinessLogic;
            PrivateSectorRepository = privateSectorRepository;
        }

        public ISharedBusinessLogic SharedBusinessLogic { get; }
        public ISubmissionBusinessLogic SubmissionBusinessLogic { get; }
        public IScopeBusinessLogic ScopeBusinessLogic { get; }
        public IDraftFileBusinessLogic DraftFileBusinessLogic { get; }
        public IPagedRepository<EmployerRecord> PrivateSectorRepository { get; }
    }
}