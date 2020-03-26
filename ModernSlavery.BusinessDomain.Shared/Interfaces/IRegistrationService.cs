using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IRegistrationService
    {
        Registration.IRegistrationBusinessLogic RegistrationBusinessLogic { get; }
        IAuditLogger BadSicLog { get; }
        IAuditLogger RegistrationLog { get; }
        IPinInThePostService PinInThePostService { get; }
        IPostcodeChecker PostcodeChecker { get; }
        ISharedBusinessLogic SharedBusinessLogic { get; }
        IOrganisationBusinessLogic OrganisationBusinessLogic { get; }
        IScopeBusinessLogic ScopeBusinessLogic { get; }
        ISearchBusinessLogic SearchBusinessLogic { get; }
        IUserRepository UserRepository { get; }
        IPagedRepository<EmployerRecord> PrivateSectorRepository { get; }
        IPagedRepository<EmployerRecord> PublicSectorRepository { get; }
    }
}