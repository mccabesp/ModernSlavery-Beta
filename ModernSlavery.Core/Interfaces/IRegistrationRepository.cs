using System.Threading.Tasks;
using ModernSlavery.Entities;

namespace ModernSlavery.Core.Interfaces
{
    public interface IRegistrationRepository : IDataTransaction
    {
        Task RemoveRegistrationAsync(UserOrganisation userOrgToUnregister, User actionByUser);

        Task RemoveRetiredUserRegistrationsAsync(User userToRetire, User actionByUser);
    }
}