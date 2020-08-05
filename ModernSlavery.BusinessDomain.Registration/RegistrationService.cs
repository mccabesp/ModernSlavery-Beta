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
            PostcodeChecker = postcodeChecker;
        }

        public async Task SetIsUkAddressesAsync()
        {
            var addresses = SharedBusinessLogic.DataRepository.GetAll<OrganisationAddress>().Where(a => a.IsUkAddress==null);
            foreach (var org in addresses) await SetIsUkAddressAsync(org);
        }

        public async Task SetIsUkAddressAsync(OrganisationAddress address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            if (string.IsNullOrWhiteSpace(address.PostCode)) throw new ArgumentNullException(nameof(address.PostCode));

            //Check if the address is a valid UK postcode
            address.IsUkAddress = await PostcodeChecker.IsValidPostcode(address.PostCode);

            //Save the address
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();
        }
    }
}