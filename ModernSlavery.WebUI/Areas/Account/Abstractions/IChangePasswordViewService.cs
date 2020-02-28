using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Entities;

namespace ModernSlavery.WebUI.Areas.Account.Abstractions
{

    public interface IChangePasswordViewService
    {

        Task<ModelStateDictionary> ChangePasswordAsync(User currentUser, string currentPassword, string newPassword);

    }

}
