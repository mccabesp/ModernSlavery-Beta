using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Models.LogModels;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IAdminService
    {
        IDataImporter DataImporter { get; }
        IAuditLogger ManualChangeLog { get; }
        IAuditLogger BadSicLog { get; }
        IAuditLogger RegistrationLog { get; }
        IShortCodesRepository ShortCodesRepository { get; }
        IOrganisationBusinessLogic OrganisationBusinessLogic { get; set; }
        ISearchBusinessLogic SearchBusinessLogic { get; set; }
        IUserRepository UserRepository { get; }
        IPagedRepository<OrganisationRecord> PrivateSectorRepository { get; }
        IPagedRepository<OrganisationRecord> PublicSectorRepository { get; }
        IQueue ExecuteWebjobQueue { get; }
        ISearchRepository<OrganisationSearchModel> OrganisationSearchRepository { get; }
        ISharedBusinessLogic SharedBusinessLogic { get; }
    }
}