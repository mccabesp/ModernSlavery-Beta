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
            [KeyFilter("Private")] IPagedRepository<OrganisationRecord> privateSectorRepository,
            [KeyFilter("Public")] IPagedRepository<OrganisationRecord> publicSectorRepository,
            ISearchRepository<OrganisationSearchModel> organisationSearchRepository,
            ISharedBusinessLogic sharedBusinessLogic,
            IDataImporter dataImporter
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

            OrganisationSearchRepository = organisationSearchRepository;
            SharedBusinessLogic = sharedBusinessLogic;

            DataImporter = dataImporter;
        }

        public IDataImporter DataImporter { get; }
        public IAuditLogger ManualChangeLog { get; }
        public IAuditLogger BadSicLog { get; }
        public IAuditLogger RegistrationLog { get; }

        public IShortCodesRepository ShortCodesRepository { get; }
        public IOrganisationBusinessLogic OrganisationBusinessLogic { get; set; }
        public ISearchBusinessLogic SearchBusinessLogic { get; set; }
        public ISubmissionBusinessLogic SubmissionBusinessLogic { get; }
        public IUserRepository UserRepository { get; }
        public IPagedRepository<OrganisationRecord> PrivateSectorRepository { get; }
        public IPagedRepository<OrganisationRecord> PublicSectorRepository { get; }
        public IQueue ExecuteWebjobQueue { get; }

        public ISearchRepository<OrganisationSearchModel> OrganisationSearchRepository { get; }
        public ISharedBusinessLogic SharedBusinessLogic { get; }

        public async Task LogSubmission(IOrderedEnumerable<SubmissionLogModel> logRecords)
        {
            await SubmissionBusinessLogic.SubmissionLog.WriteAsync(logRecords);
        }
    }
}