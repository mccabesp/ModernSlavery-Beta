using Autofac.Features.AttributeFilters;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Infrastructure;
using ModernSlavery.SharedKernel;

namespace ModernSlavery.BusinessLogic.Register
{
    public interface IRegistrationService
    {
        IRegistrationBusinessLogic RegistrationBusinessLogic { get; }

        ILogRecordLogger BadSicLog { get; }
        ILogRecordLogger RegistrationLog { get; }

        public IPinInThePostService PinInThePostService { get; }
        public IPostcodeChecker PostcodeChecker { get; }

        public ICommonBusinessLogic CommonBusinessLogic { get; }
        public IOrganisationBusinessLogic OrganisationBusinessLogic { get; }
        public IScopeBusinessLogic ScopeBusinessLogic { get; }
        public ISearchBusinessLogic SearchBusinessLogic { get; }
        public IUserRepository UserRepository { get; }
        public IPagedRepository<EmployerRecord> PrivateSectorRepository { get; }
        public IPagedRepository<EmployerRecord> PublicSectorRepository { get; }
    }

    public class RegistrationService : IRegistrationService
    {
        public RegistrationService(
            [KeyFilter(Filenames.BadSicLog)] ILogRecordLogger badSicLog,
            [KeyFilter(Filenames.RegistrationLog)] ILogRecordLogger registrationLog,
            IRegistrationBusinessLogic registrationBusinessLogic,
            IScopeBusinessLogic scopeBL,
            IOrganisationBusinessLogic orgBL,
            ICommonBusinessLogic commonBusinessLogic,
            ISearchBusinessLogic searchBusinessLogic,
            IUserRepository userRepository,
            IPinInThePostService pinInThePostService,
            IPostcodeChecker postcodeChecker,
            [KeyFilter("Private")] IPagedRepository<EmployerRecord> privateSectorRepository,
            [KeyFilter("Public")] IPagedRepository<EmployerRecord> publicSectorRepository
        )
        {
            RegistrationBusinessLogic = registrationBusinessLogic;
            BadSicLog = badSicLog;
            RegistrationLog = registrationLog;

            ScopeBusinessLogic = scopeBL;
            OrganisationBusinessLogic = orgBL;
            CommonBusinessLogic = commonBusinessLogic;
            SearchBusinessLogic = searchBusinessLogic;
            PrivateSectorRepository = privateSectorRepository;
            PublicSectorRepository = publicSectorRepository;
            UserRepository = userRepository;
            PinInThePostService = pinInThePostService;
            PostcodeChecker = postcodeChecker;
        }
        public IRegistrationBusinessLogic RegistrationBusinessLogic { get; }
        
        public ILogRecordLogger BadSicLog { get; }
        public ILogRecordLogger RegistrationLog { get; }

        public IPinInThePostService PinInThePostService { get; }
        public IPostcodeChecker PostcodeChecker { get; }

        public ICommonBusinessLogic CommonBusinessLogic { get; }
        public IOrganisationBusinessLogic OrganisationBusinessLogic { get; }
        public IScopeBusinessLogic ScopeBusinessLogic { get; }
        public ISearchBusinessLogic SearchBusinessLogic { get; }
        public IUserRepository UserRepository { get; }
        public IPagedRepository<EmployerRecord> PrivateSectorRepository { get; }
        public IPagedRepository<EmployerRecord> PublicSectorRepository { get; }
    }
}