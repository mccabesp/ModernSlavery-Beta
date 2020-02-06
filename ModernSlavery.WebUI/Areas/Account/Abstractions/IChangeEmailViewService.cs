using System.Threading.Tasks;
using ModernSlavery.Database;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ModernSlavery.WebUI.Areas.Account.Abstractions
{
    public interface IChangeEmailViewService
    {

        Task<ModelStateDictionary> InitiateChangeEmailAsync(string newEmailAddress, User currentUser);

        Task<ModelStateDictionary> CompleteChangeEmailAsync(string code, User currentUser);

    }

}
