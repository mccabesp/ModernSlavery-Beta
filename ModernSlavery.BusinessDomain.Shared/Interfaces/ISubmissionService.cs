using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface ISubmissionService
    {
        ISharedBusinessLogic SharedBusinessLogic { get; }
        ISubmissionBusinessLogic SubmissionBusinessLogic { get; }
        IScopeBusinessLogic ScopeBusinessLogic { get; }
        IDraftFileBusinessLogic DraftFileBusinessLogic { get; }
        IPagedRepository<EmployerRecord> PrivateSectorRepository { get; }
    }
}