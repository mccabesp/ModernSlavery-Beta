using Autofac.Features.AttributeFilters;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.SharedKernel;

namespace ModernSlavery.BusinessLogic.Admin
{
    public interface IAdminService
    {
        IRecordLogger ManualChangeLog { get; }
        IRecordLogger BadSicLog { get; }
        IRecordLogger RegistrationLog { get; }
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
        ICommonBusinessLogic CommonBusinessLogic { get; }
    }

    public class AdminService : IAdminService
    {
        public AdminService(
            [KeyFilter(Filenames.ManualChangeLog)] IRecordLogger manualChangeLog,
            [KeyFilter(Filenames.BadSicLog)] IRecordLogger badSicLog,
            [KeyFilter(Filenames.RegistrationLog)] IRecordLogger registrationLog,
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
            ICommonBusinessLogic commonBusinessLogic
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
            CommonBusinessLogic = commonBusinessLogic;
        }

        public IRecordLogger ManualChangeLog { get; }
        public IRecordLogger BadSicLog { get; }
        public IRecordLogger RegistrationLog { get; }

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
        public ICommonBusinessLogic CommonBusinessLogic { get; }
    }
}