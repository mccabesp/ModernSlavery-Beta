using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.Account.Interfaces;
using ModernSlavery.WebUI.Account.Models.ChangePassword;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Resources;

namespace ModernSlavery.WebUI.Account.Controllers
{

    [Area("Account")]
    [Route("manage-account")]
    public class ChangePasswordController : BaseController
    {

        public ChangePasswordController(
            IChangePasswordViewService changePasswordService,
            ILogger<ChangePasswordController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            ChangePasswordService = changePasswordService;
        }

        public IChangePasswordViewService ChangePasswordService { get; }

        [HttpGet("change-password")]
        public async Task<IActionResult> ChangePassword()
        {
            var checkResult = await CheckUserRegisteredOkAsync();
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
            var checkResult = await CheckUserRegisteredOkAsync();
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
                VirtualUser,
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
