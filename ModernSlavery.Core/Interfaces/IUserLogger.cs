using System.Threading.Tasks;
using ModernSlavery.Core.Models;
using ModernSlavery.Entities;

namespace ModernSlavery.Core.Interfaces
{
    public interface IUserLogger : IRecordLogger
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