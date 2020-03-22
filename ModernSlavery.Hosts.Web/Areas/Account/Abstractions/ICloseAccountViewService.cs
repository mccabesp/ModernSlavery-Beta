using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.WebUI.Areas.Account.Abstractions
{

    public interface ICloseAccountViewService
    {

        Task<ModelStateDictionary> CloseAccountAsync(User currentUser, string currentPassword, User actionByUser);

    }

}
