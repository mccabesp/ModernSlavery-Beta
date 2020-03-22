using Autofac.Features.AttributeFilters;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel;

namespace ModernSlavery.BusinessLogic.Registration
{
    public interface IRegistrationService
    {
        IRegistrationBusinessLogic RegistrationBusinessLogic { get; }

        IRecordLogger BadSicLog { get; }
        IRecordLogger RegistrationLog { get; }

        public IPinInThePostService PinInThePostService { get; }
        public IPostcodeChecker PostcodeChecker { get; }

        public ISharedBusinessLogic SharedBusinessLogic { get; }
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
            [KeyFilter(Filenames.BadSicLog)] IRecordLogger badSicLog,
            [KeyFilter(Filenames.RegistrationLog)] IRecordLogger registrationLog,
            IRegistrationBusinessLogic registrationBusinessLogic,
            IScopeBusinessLogic scopeBL,
            IOrganisationBusinessLogic orgBL,
            ISharedBusinessLogic sharedBusinessLogic,
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
            SharedBusinessLogic = sharedBusinessLogic;
            SearchBusinessLogic = searchBusinessLogic;
            PrivateSectorRepository = privateSectorRepository;
            PublicSectorRepository = publicSectorRepository;
            UserRepository = userRepository;
            PinInThePostService = pinInThePostService;
            PostcodeChecker = postcodeChecker;
        }
        public IRegistrationBusinessLogic RegistrationBusinessLogic { get; }
        
        public IRecordLogger BadSicLog { get; }
        public IRecordLogger RegistrationLog { get; }

        public IPinInThePostService PinInThePostService { get; }
        public IPostcodeChecker PostcodeChecker { get; }

        public ISharedBusinessLogic SharedBusinessLogic { get; }
        public IOrganisationBusinessLogic OrganisationBusinessLogic { get; }
        public IScopeBusinessLogic ScopeBusinessLogic { get; }
        public ISearchBusinessLogic SearchBusinessLogic { get; }
        public IUserRepository UserRepository { get; }
        public IPagedRepository<EmployerRecord> PrivateSectorRepository { get; }
        public IPagedRepository<EmployerRecord> PublicSectorRepository { get; }
    }
}