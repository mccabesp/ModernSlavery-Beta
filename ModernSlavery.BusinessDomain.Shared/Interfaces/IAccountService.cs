using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IAccountService
    {
        IUserRepository UserRepository { get; }
        Registration.IRegistrationBusinessLogic RegistrationBusinessLogic { get; }
    }
}