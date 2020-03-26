using ModernSlavery.BusinessDomain.Registration;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.BusinessDomain.Account
{
    public interface IAccountService
    {
        IUserRepository UserRepository { get; }
        IRegistrationBusinessLogic RegistrationBusinessLogic { get; }
    }

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