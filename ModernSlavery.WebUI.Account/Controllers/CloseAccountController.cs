using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.WebUI.Account.Interfaces;
using ModernSlavery.WebUI.Account.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Account.Controllers
{
    [Area("Account")]
    [Route("manage-account")]
    public class CloseAccountController : BaseController
    {
        public CloseAccountController(
            ICloseAccountViewService closeAccountService,
            ILogger<CloseAccountController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) :
            base(logger, webService, sharedBusinessLogic)
        {
            CloseAccountService = closeAccountService;
        }

        public ICloseAccountViewService CloseAccountService { get; }

        [HttpGet("close-account")]
        public async Task<IActionResult> CloseAccount()
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            return View(new CloseAccountViewModel
                {IsSoleUserOfOneOrMoreOrganisations = VirtualUser.IsSoleUserOfOneOrMoreOrganisations()});
        }

        [HttpPost("close-account")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseAccount([FromForm] CloseAccountViewModel formData)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            // prevent impersonation
            if (IsImpersonatingUser) RedirectToAction<AccountController>(nameof(AccountController.ManageAccount));

            // return to page if there are errors
            if (ModelState.IsValid == false) return View(nameof(CloseAccount), formData);

            // execute change password process
            var errors = await CloseAccountService.CloseAccountAsync(VirtualUser, formData.EnterPassword, VirtualUser);
            if (errors.ErrorCount > 0)
            {
                ModelState.Merge(errors);
                return View(nameof(CloseAccount), formData);
            }

            // force sign-out then redirect to completed page
            var redirectUrl = Url.Action<CloseAccountController>(nameof(CloseAccountCompleted));

            // logout the
            return await LogoutUser(redirectUrl);
        }

        [AllowAnonymous]
        [HttpGet("close-account-completed")]
        public IActionResult CloseAccountCompleted()
        {
            return View("CloseAccountCompleted");
        }
    }
}