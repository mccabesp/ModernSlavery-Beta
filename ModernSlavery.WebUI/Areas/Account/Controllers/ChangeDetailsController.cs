using System.Threading.Tasks;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Areas.Account.Abstractions;
using ModernSlavery.WebUI.Areas.Account.Resources;
using ModernSlavery.WebUI.Areas.Account.ViewModels;
using ModernSlavery.WebUI.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Abstractions;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.Entities;

namespace ModernSlavery.WebUI.Areas.Account.Controllers
{

    [Area("Account")]
    [Route("manage-account")]
    public class ChangeDetailsController : BaseController
    {

        public ChangeDetailsController(
            IChangeDetailsViewService changeDetailsService,
            ILogger<ChangeDetailsController> logger,
            IWebService webService,
            IDataRepository dataRepository) : base(logger, webService, dataRepository)
        {
            ChangeDetailsService = changeDetailsService;
        }

        public IChangeDetailsViewService ChangeDetailsService { get; }

        [HttpGet("change-details")]
        public IActionResult ChangeDetails()
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

            // map the user to the edit view model
            var model = AutoMapper.Map<ChangeDetailsViewModel>(currentUser);

            return View(model);
        }

        [HttpPost("change-details")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeDetails([FromForm] ChangeDetailsViewModel formData)
        {
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // Validate fields
            if (ModelState.IsValid == false)
            {
                return View(nameof(ChangeDetails), formData);
            }

            // Execute change details
            bool success = await ChangeDetailsService.ChangeDetailsAsync(formData, currentUser);

            // set success alert flag
            if (success)
            {
                TempData.Add(nameof(AccountResources.ChangeDetailsSuccessAlert), true);
            }

            // go to manage account page
            return this.RedirectToAction<ManageAccountController>(nameof(ManageAccountController.ManageAccount));
        }

    }

}
