using System.Threading.Tasks;
using ModernSlavery.Entities;

namespace ModernSlavery.Core.Interfaces
{
    public interface IRegistrationLogRecord : ILogRecordLogger
    {

        Task LogUnregisteredAsync(UserOrganisation unregisteredUserOrg, string actionByEmailAddress);

        Task LogUnregisteredSelfAsync(UserOrganisation unregisteredUserOrg, string actionByEmailAddress);

        Task LogUserAccountClosedAsync(UserOrganisation userOrgToUnregister, string actionByEmailAddress);

    }
}