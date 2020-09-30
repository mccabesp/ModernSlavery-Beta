using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface ISubmissionService
    {
        ISharedBusinessLogic SharedBusinessLogic { get; }
        ISubmissionBusinessLogic SubmissionBusinessLogic { get; }
        IScopeBusinessLogic ScopeBusinessLogic { get; }
        IPagedRepository<OrganisationRecord> PrivateSectorRepository { get; }
    }
}