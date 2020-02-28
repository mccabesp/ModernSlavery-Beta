using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Entities;

namespace ModernSlavery.WebUI.Areas.Account.Abstractions
{
    public interface IChangeEmailViewService
    {

        Task<ModelStateDictionary> InitiateChangeEmailAsync(string newEmailAddress, User currentUser);

        Task<ModelStateDictionary> CompleteChangeEmailAsync(string code, User currentUser);

    }

}
