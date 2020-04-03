using System;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.BusinessDomain.Account
{
    public class AccountService : IAccountService
    {
        public AccountService(IUserRepository userRepository, IRegistrationBusinessLogic registrationBusinessLogic, ISharedBusinessLogic sharedBusinessLogic)
        {
            UserRepository = userRepository;
            RegistrationBusinessLogic = registrationBusinessLogic;
            SharedBusinessLogic = sharedBusinessLogic;
        }

        public ISharedBusinessLogic SharedBusinessLogic { get; }
        public IUserRepository UserRepository { get; }
        public IRegistrationBusinessLogic RegistrationBusinessLogic { get; }
    }
}