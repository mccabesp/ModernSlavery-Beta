using System.Threading.Tasks;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.Areas.Account.ViewModels;
using ModernSlavery.WebUI.Areas.Account.ViewModels.ChangeDetails;

namespace ModernSlavery.WebUI.Areas.Account.Abstractions
{

    public interface IChangeDetailsViewService
    {

        Task<bool> ChangeDetailsAsync(ChangeDetailsViewModel newDetails, User currentUser);

    }

}
