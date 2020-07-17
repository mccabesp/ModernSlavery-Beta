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
        ISubmissionBusinessLogic SubmissionBusinessLogic { get; }
        IUserRepository UserRepository { get; }
        IPagedRepository<EmployerRecord> PrivateSectorRepository { get; }
        IPagedRepository<EmployerRecord> PublicSectorRepository { get; }
        IQueue ExecuteWebjobQueue { get; }
        ISearchRepository<EmployerSearchModel> EmployerSearchRepository { get; }
        ISearchRepository<SicCodeSearchModel> SicCodeSearchRepository { get; }
        ISharedBusinessLogic SharedBusinessLogic { get; }
        Task LogSubmission(IOrderedEnumerable<SubmissionLogModel> logRecords);
    }
}