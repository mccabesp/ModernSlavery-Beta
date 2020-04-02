using ModernSlavery.Core.Entities;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IAuthorisationBusinessLogic
    {
        bool IsAdministrator(User user);
        bool IsSuperAdministrator(User user);
        bool IsDatabaseAdministrator(User user);
    }
}