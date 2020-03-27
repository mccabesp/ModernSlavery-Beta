using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.WebUI.Account.Interfaces
{
    public interface IChangePasswordViewService
    {
        Task<ModelStateDictionary> ChangePasswordAsync(User currentUser, string currentPassword, string newPassword);
    }
}