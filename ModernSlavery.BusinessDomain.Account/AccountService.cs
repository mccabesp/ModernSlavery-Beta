using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Interfaces;
using IRegistrationBusinessLogic = ModernSlavery.BusinessDomain.Registration.IRegistrationBusinessLogic;

namespace ModernSlavery.BusinessDomain.Account
{
    public class AccountService : IAccountService
    {
        public AccountService(IUserRepository userRepository, IRegistrationBusinessLogic registrationBusinessLogic)
        {
            UserRepository = userRepository;
            RegistrationBusinessLogic = registrationBusinessLogic;
        }

        public IUserRepository UserRepository { get; }
        public IRegistrationBusinessLogic RegistrationBusinessLogic { get; }
    }
}