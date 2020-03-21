using System.Threading.Tasks;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Areas.Account.Abstractions;
using ModernSlavery.WebUI.Areas.Account.Resources;
using ModernSlavery.WebUI.Areas.Account.ViewModels;
using ModernSlavery.WebUI.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessLogic;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.Entities;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Areas.Account.Controllers
{

    [Area("Account")]
    [Route("manage-account")]
    public class ChangePasswordController : BaseController
    {

        public ChangePasswordController(
            IChangePasswordViewService changePasswordService,
            ILogger<ChangePasswordController> logger, IWebService webService, ICommonBusinessLogic commonBusinessLogic) : base(logger, webService, commonBusinessLogic)
        {
            ChangePasswordService = changePasswordService;
        }

        public IChangePasswordViewService ChangePasswordService { get; }

        [HttpGet("change-password")]
        public IActionResult ChangePassword()
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

            return View(new ChangePasswordViewModel());
        }

        [HttpPost("change-password")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordViewModel formData)
        {
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // return to page if there are errors
            if (ModelState.IsValid == false)
            {
                return View(nameof(ChangePassword), formData);
            }

            // execute change password process
            ModelStateDictionary errors = await ChangePasswordService.ChangePasswordAsync(
                currentUser,
                formData.CurrentPassword,
                formData.NewPassword);

            if (errors.ErrorCount > 0)
            {
                ModelState.Merge(errors);
                return View(nameof(ChangePassword), formData);
            }

            // set success alert flag
            TempData.Add(nameof(AccountResources.ChangePasswordSuccessAlert), true);

            // go to manage account page
            return this.RedirectToAction<ManageAccountController>(nameof(ManageAccountController.ManageAccount));
        }

    }

}
