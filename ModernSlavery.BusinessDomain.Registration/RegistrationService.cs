using Autofac.Features.AttributeFilters;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.BusinessDomain.Registration
{
    public class RegistrationService : IRegistrationService
    {
        public IAuditLogger BadSicLog { get; }
        public IAuditLogger RegistrationLog { get; }
        public IRegistrationBusinessLogic RegistrationBusinessLogic { get; }
        public IScopeBusinessLogic ScopeBusinessLogic { get; }
        public IOrganisationBusinessLogic OrganisationBusinessLogic { get; }
        public ISharedBusinessLogic SharedBusinessLogic { get; }
        public ISearchBusinessLogic SearchBusinessLogic { get; }
        public IPagedRepository<EmployerRecord> PrivateSectorRepository { get; }
        public IPagedRepository<EmployerRecord> PublicSectorRepository { get; }
        public IUserRepository UserRepository { get; }
        public IPinInThePostService PinInThePostService { get; }
        public IPostcodeChecker PostcodeChecker { get; }


        public RegistrationService(
            [KeyFilter(Filenames.BadSicLog)] IAuditLogger badSicLog,
            [KeyFilter(Filenames.RegistrationLog)] IAuditLogger registrationLog,
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
        }
    }
}