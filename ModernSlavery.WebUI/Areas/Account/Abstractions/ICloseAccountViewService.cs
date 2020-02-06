using System.Threading.Tasks;
using ModernSlavery.Database;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ModernSlavery.WebUI.Areas.Account.Abstractions
{

    public interface ICloseAccountViewService
    {

        Task<ModelStateDictionary> CloseAccountAsync(User currentUser, string currentPassword, User actionByUser);

    }

}
