using System.Threading.Tasks;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.Account.Models;

namespace ModernSlavery.WebUI.Account.Interfaces
{
    public interface IChangeDetailsViewService
    {
        Task<bool> ChangeDetailsAsync(ChangeDetailsViewModel newDetails, User currentUser);
    }
}