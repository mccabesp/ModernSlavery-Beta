using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.WebUI.Account.Interfaces
{

    public interface ICloseAccountViewService
    {

        Task<ModelStateDictionary> CloseAccountAsync(User currentUser, string currentPassword, User actionByUser);

    }

}
