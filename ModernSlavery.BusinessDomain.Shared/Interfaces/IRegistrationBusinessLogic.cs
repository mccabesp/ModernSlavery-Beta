using System.Threading.Tasks;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IRegistrationBusinessLogic
    {
        Task RemoveRetiredUserRegistrationsAsync(User userToRetire, User actionByUser);
        Task RemoveRegistrationAsync(UserOrganisation userOrgToUnregister, User actionByUser);
    }
}