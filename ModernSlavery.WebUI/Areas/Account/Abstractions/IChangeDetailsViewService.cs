using System.Threading.Tasks;
using ModernSlavery.Database;
using ModernSlavery.WebUI.Areas.Account.ViewModels;

namespace ModernSlavery.WebUI.Areas.Account.Abstractions
{

    public interface IChangeDetailsViewService
    {

        Task<bool> ChangeDetailsAsync(ChangeDetailsViewModel newDetails, User currentUser);

    }

}
