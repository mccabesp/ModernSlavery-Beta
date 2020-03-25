using System.Threading.Tasks;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Core.Interfaces
{
    public interface IUserLogger : IAuditLogger
    {
        Task LogEmailChangedAsync(string oldEmailAddress, string newEmailAddress, User userToUpdate,
            string actionByEmailAddress);

        Task LogPasswordChangedAsync(User userToUpdate, string actionByEmailAddress);

        Task LogDetailsChangedAsync(UpdateDetailsModel originalDetails,
            UpdateDetailsModel changeDetails,
            User userToUpdate,
            string emailAddress);

        Task LogUserRetiredAsync(User userToUpdate, string actionByEmailAddress);
    }
}