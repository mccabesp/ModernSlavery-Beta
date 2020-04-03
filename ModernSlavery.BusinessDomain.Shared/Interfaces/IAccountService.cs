using System;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IAccountService
    {
        ISharedBusinessLogic SharedBusinessLogic { get; }
        IUserRepository UserRepository { get; }
        IRegistrationBusinessLogic RegistrationBusinessLogic { get; }
    }
}