using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.WebUI.Account.Interfaces;
using ModernSlavery.WebUI.Account.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Resources;

namespace ModernSlavery.WebUI.Account.Controllers
{
    [Route("manage-account")]
    public class ChangeDetailsController : BaseController
    {
        public ChangeDetailsController(
            IChangeDetailsViewService changeDetailsService,
            ILogger<ChangeDetailsController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) :
            base(logger, webService, sharedBusinessLogic)
        {
            ChangeDetailsService = changeDetailsService;
        }

        public IChangeDetailsViewService ChangeDetailsService { get; }

        [HttpGet("change-details")]
        public async Task<IActionResult> ChangeDetailsAsync()
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            // prevent impersonation
            if (IsImpersonatingUser) RedirectToAction<AccountController>(nameof(AccountController.ManageAccount));

            // map the user to the edit view model
            var model = AutoMapper.Map<ChangeDetailsViewModel>(VirtualUser);

            return View(model);
        }

        [HttpPost("change-details")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeDetails([FromForm] ChangeDetailsViewModel formData)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            // Validate fields
            if (ModelState.IsValid == false) return View(nameof(ChangeDetails), formData);

            // Execute change details
            var success = await ChangeDetailsService.ChangeDetailsAsync(formData, VirtualUser);

            // set success alert flag
            if (success) TempData.Add(nameof(AccountResources.ChangeDetailsSuccessAlert), true);

            // go to manage account page
            return RedirectToAction<AccountController>(nameof(AccountController.ManageAccount));
        }
    }
}