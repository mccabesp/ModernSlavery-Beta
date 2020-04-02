using System;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;
using IRegistrationBusinessLogic = ModernSlavery.BusinessDomain.Registration.IRegistrationBusinessLogic;

namespace ModernSlavery.BusinessDomain.Account
{
    public class AccountService : IAccountService
    {
        public AccountService(IUserRepository userRepository, IRegistrationBusinessLogic registrationBusinessLogic, SharedBusinessLogic sharedBusinessLogic)
        {
            UserRepository = userRepository;
            RegistrationBusinessLogic = registrationBusinessLogic;
            SharedBusinessLogic = sharedBusinessLogic;
        }

        public SharedBusinessLogic SharedBusinessLogic { get; }
        public IUserRepository UserRepository { get; }
        public IRegistrationBusinessLogic RegistrationBusinessLogic { get; }

        public TimeSpan GetUserLockRemaining(User user) =>
            user.LoginDate == null || user.LoginAttempts < SharedBusinessLogic.SharedOptions.MaxLoginAttempts
                ? TimeSpan.Zero
                : user.LoginDate.Value.AddMinutes(SharedBusinessLogic.SharedOptions.LockoutMinutes) - VirtualDateTime.Now;

        
    }
}