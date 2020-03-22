using System.Threading.Tasks;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Areas.Account.Abstractions;
using ModernSlavery.WebUI.Areas.Account.ViewModels;
using ModernSlavery.WebUI.Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessLogic;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Admin.Controllers;
using ModernSlavery.WebUI.Areas.Account.ViewModels.CloseAccount;
using ModernSlavery.WebUI.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Areas.Account.Controllers
{

    [Area("Account")]
    [Route("manage-account")]
    public class CloseAccountController : BaseController
    {

        public CloseAccountController(
            ICloseAccountViewService closeAccountService,
            ILogger<CloseAccountController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            CloseAccountService = closeAccountService;
        }

        public ICloseAccountViewService CloseAccountService { get; }

        [HttpGet("close-account")]
        public IActionResult CloseAccount()
        {
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            return View(new CloseAccountViewModel {IsSoleUserOfOneOrMoreOrganisations = currentUser.IsSoleUserOfOneOrMoreOrganisations()});
        }

        [HttpPost("close-account")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseAccount([FromForm] CloseAccountViewModel formData)
        {
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // prevent impersonation
            if (IsImpersonatingUser)
            {
                this.RedirectToAction<ManageAccountController>(nameof(ManageAccountController.ManageAccount));
            }

            // return to page if there are errors
            if (ModelState.IsValid == false)
            {
                return View(nameof(CloseAccount), formData);
            }

            // execute change password process
            ModelStateDictionary errors = await CloseAccountService.CloseAccountAsync(currentUser, formData.EnterPassword, currentUser);
            if (errors.ErrorCount > 0)
            {
                ModelState.Merge(errors);
                return View(nameof(CloseAccount), formData);
            }

            // force sign-out then redirect to completed page
            string redirectUrl = Url.Action<CloseAccountController>(nameof(CloseAccountCompleted));

            // logout the
            return LogoutUser(redirectUrl);
        }

        [AllowAnonymous]
        [HttpGet("close-account-completed")]
        public IActionResult CloseAccountCompleted()
        {
            return View("CloseAccountCompleted");
        }

    }

}
