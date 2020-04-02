using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Models.LogModels;

namespace ModernSlavery.BusinessDomain.Admin
{
    public class AdminService : IAdminService
    {
        public AdminService(
            [KeyFilter(Filenames.ManualChangeLog)] IAuditLogger manualChangeLog,
            [KeyFilter(Filenames.BadSicLog)] IAuditLogger badSicLog,
            [KeyFilter(Filenames.RegistrationLog)] IAuditLogger registrationLog,
            IShortCodesRepository shortCodesRepository,
            IOrganisationBusinessLogic organisationBusinessLogic,
            ISearchBusinessLogic searchBusinessLogic,
            ISubmissionBusinessLogic submissionBusinessLogic,
            IUserRepository userRepository,
            [KeyFilter(QueueNames.ExecuteWebJob)] IQueue executeWebjobQueue,
            [KeyFilter("Private")] IPagedRepository<EmployerRecord> privateSectorRepository,
            [KeyFilter("Public")] IPagedRepository<EmployerRecord> publicSectorRepository,
            ISearchRepository<EmployerSearchModel> employerSearchRepository,
            ISearchRepository<SicCodeSearchModel> sicCodeSearchRepository,
            ISharedBusinessLogic sharedBusinessLogic
        )
        {
            ManualChangeLog = manualChangeLog;
            BadSicLog = badSicLog;
            RegistrationLog = registrationLog;

            ShortCodesRepository = shortCodesRepository;
            OrganisationBusinessLogic = organisationBusinessLogic;
            SearchBusinessLogic = searchBusinessLogic;
            SubmissionBusinessLogic = submissionBusinessLogic;
            UserRepository = userRepository;
            ExecuteWebjobQueue = executeWebjobQueue;
            PrivateSectorRepository = privateSectorRepository;
            PublicSectorRepository = publicSectorRepository;

            EmployerSearchRepository = employerSearchRepository;
            SicCodeSearchRepository = sicCodeSearchRepository;
            SharedBusinessLogic = sharedBusinessLogic;
        }

        public IAuditLogger ManualChangeLog { get; }
        public IAuditLogger BadSicLog { get; }
        public IAuditLogger RegistrationLog { get; }

        public IShortCodesRepository ShortCodesRepository { get; }
        public IOrganisationBusinessLogic OrganisationBusinessLogic { get; set; }
        public ISearchBusinessLogic SearchBusinessLogic { get; set; }
        public ISubmissionBusinessLogic SubmissionBusinessLogic { get; }
        public IUserRepository UserRepository { get; }
        public IPagedRepository<EmployerRecord> PrivateSectorRepository { get; }
        public IPagedRepository<EmployerRecord> PublicSectorRepository { get; }
        public IQueue ExecuteWebjobQueue { get; }

        public ISearchRepository<EmployerSearchModel> EmployerSearchRepository { get; }
        public ISearchRepository<SicCodeSearchModel> SicCodeSearchRepository { get; }
        public ISharedBusinessLogic SharedBusinessLogic { get; }

        public async Task LogSubmission(IOrderedEnumerable<SubmissionLogModel> logRecords)
        {
            await SubmissionBusinessLogic.SubmissionLog.WriteAsync(logRecords);
        }
    }
}