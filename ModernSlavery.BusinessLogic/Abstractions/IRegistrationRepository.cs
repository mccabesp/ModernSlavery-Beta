using System.Threading.Tasks;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Entities;

namespace ModernSlavery.BusinessLogic.Account.Abstractions
{

    public interface IRegistrationRepository : IDataTransaction
    {

        Task RemoveRegistrationAsync(UserOrganisation userOrgToUnregister, User actionByUser);

        Task RemoveRetiredUserRegistrationsAsync(User userToRetire, User actionByUser);

    }

}
