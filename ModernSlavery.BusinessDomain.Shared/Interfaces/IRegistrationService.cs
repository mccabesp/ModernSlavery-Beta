using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using System.Threading.Tasks;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IRegistrationService
    {
        IRegistrationBusinessLogic RegistrationBusinessLogic { get; }
        IAuditLogger BadSicLog { get; }
        IAuditLogger RegistrationLog { get; }
        IPinInThePostService PinInThePostService { get; }
        ISharedBusinessLogic SharedBusinessLogic { get; }
        IOrganisationBusinessLogic OrganisationBusinessLogic { get; }
        IScopeBusinessLogic ScopeBusinessLogic { get; }
        ISearchBusinessLogic SearchBusinessLogic { get; }
        IUserRepository UserRepository { get; }
        IPagedRepository<OrganisationRecord> PrivateSectorRepository { get; }
        IPagedRepository<OrganisationRecord> PublicSectorRepository { get; }
    }
}