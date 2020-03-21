using System.Threading.Tasks;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Extensions;
using ModernSlavery.WebUI.Areas.Account.Abstractions;
using ModernSlavery.WebUI.Areas.Account.ViewModels;
using ModernSlavery.WebUI.Classes;
using Microsoft.AspNetCore.Authorization;
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
    public class ChangeEmailController : BaseController
    {

        public ChangeEmailController(
            IChangeEmailViewService changeEmailService,
            ILogger<ChangeEmailController> logger, IWebService webService, ICommonBusinessLogic commonBusinessLogic) : base(logger, webService, commonBusinessLogic)
        {
            ChangeEmailService = changeEmailService;
        }

        public IChangeEmailViewService ChangeEmailService { get; }

        [HttpGet("change-email")]
        public IActionResult ChangeEmail()
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

            return View(new ChangeEmailViewModel());
        }

        [HttpPost("change-email")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail([FromForm] ChangeEmailViewModel formData)
        {
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // return to page if there are errors
            if (ModelState.IsValid == false)
            {
                return View(nameof(ChangeEmail), formData);
            }

            // initialize change email process
            ModelStateDictionary errors = await ChangeEmailService.InitiateChangeEmailAsync(formData.EmailAddress, currentUser);
            if (errors.ErrorCount > 0)
            {
                ModelState.Merge(errors);
                return View(nameof(ChangeEmail), formData);
            }

            // confirm email change link sent
            var changeEmailModel = new ChangeEmailStatusViewModel {OldEmail = currentUser.EmailAddress, NewEmail = formData.EmailAddress};

            // go to pending page
            return RedirectToAction(nameof(ChangeEmailPending), new {data = Encryption.EncryptModel(changeEmailModel)});
        }

        [HttpGet("change-email-pending")]
        public IActionResult ChangeEmailPending(string data)
        {
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            var changeEmailModel = Encryption.DecryptModel<ChangeEmailStatusViewModel>(data);

            return View(nameof(ChangeEmailPending), changeEmailModel);
        }

        [AllowAnonymous]
        [Route("verify-change-email")]
        [HttpGet]
        public IActionResult VerifyChangeEmail(string code)
        {
            // if not logged in go straight to CompleteChangeEmailAsync
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null && checkResult is ChallengeResult)
            {
                return base.RedirectToAction(
                    nameof(CompleteChangeEmailAsync),
                    new {code});
            }

            // force sign-out then prompt sign-in before confirming email
            string redirectUrl = Url.Action<ChangeEmailController>(
                nameof(CompleteChangeEmailAsync),
                new {code},
                "https");

            return LogoutUser(redirectUrl);
        }

        [Route("complete-change-email")]
        [HttpGet]
        public async Task<IActionResult> CompleteChangeEmailAsync(string code)
        {
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // complete email change
            ModelStateDictionary errors = await ChangeEmailService.CompleteChangeEmailAsync(code, currentUser);
            if (errors.ErrorCount > 0)
            {
                // show failed reason
                ModelState.Merge(errors);
                return View("ChangeEmailFailed");
            }

            // show success
            var changeEmailCompletedModel = new ChangeEmailStatusViewModel {NewEmail = currentUser.EmailAddress};

            return View("ChangeEmailCompleted", changeEmailCompletedModel);
        }

    }

}
