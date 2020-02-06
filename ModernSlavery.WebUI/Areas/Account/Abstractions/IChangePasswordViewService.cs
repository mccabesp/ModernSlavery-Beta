using System.Threading.Tasks;
using ModernSlavery.Database;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ModernSlavery.WebUI.Areas.Account.Abstractions
{

    public interface IChangePasswordViewService
    {

        Task<ModelStateDictionary> ChangePasswordAsync(User currentUser, string currentPassword, string newPassword);

    }

}
