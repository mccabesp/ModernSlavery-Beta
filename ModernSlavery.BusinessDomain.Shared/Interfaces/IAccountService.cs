using System;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IAccountService
    {
        SharedBusinessLogic SharedBusinessLogic { get; }
        IUserRepository UserRepository { get; }
        Registration.IRegistrationBusinessLogic RegistrationBusinessLogic { get; }
        TimeSpan GetUserLockRemaining(User user);
    }
}