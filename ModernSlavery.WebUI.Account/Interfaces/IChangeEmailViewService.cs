using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.WebUI.Account.Interfaces
{
    public interface IChangeEmailViewService
    {
        Task<ModelStateDictionary> InitiateChangeEmailAsync(string newEmailAddress, User currentUser);

        Task<ModelStateDictionary> CompleteChangeEmailAsync(string code, User currentUser);
    }
}