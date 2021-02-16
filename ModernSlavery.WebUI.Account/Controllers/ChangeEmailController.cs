﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Account.Interfaces;
using ModernSlavery.WebUI.Account.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Classes.UrlHelper;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Account.Controllers
{
    [Area("Account")]
    [Route("manage-account")]
    public class ChangeEmailController : BaseController
    {
        public ChangeEmailController(
            IChangeEmailViewService changeEmailService,
            ILogger<ChangeEmailController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) :
            base(logger, webService, sharedBusinessLogic)
        {
            ChangeEmailService = changeEmailService;
        }

        public IChangeEmailViewService ChangeEmailService { get; }

        [HttpGet("change-email")]
        public async Task<IActionResult> ChangeEmail()
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            // prevent impersonation
            if (IsImpersonatingUser) RedirectToAction<AccountController>(nameof(AccountController.ManageAccount));

            return View(new ChangeEmailViewModel());
        }

        [HttpPost("change-email")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail([FromForm] ChangeEmailViewModel formData)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            // return to page if there are errors
            if (ModelState.IsValid == false) return View(nameof(ChangeEmail), formData);

            // initialize change email process
            var errors = await ChangeEmailService.InitiateChangeEmailAsync(formData.EmailAddress, VirtualUser);
            if (errors.ErrorCount > 0)
            {
                ModelState.Merge(errors);
                return View(nameof(ChangeEmail), formData);
            }

            // confirm email change link sent
            var changeEmailModel = new ChangeEmailStatusViewModel
                {OldEmail = VirtualUser.EmailAddress, NewEmail = formData.EmailAddress};

            // go to pending page
            return RedirectToAction(nameof(ChangeEmailPending), new {data = Encryption.EncryptModel(changeEmailModel)});
        }

        [HttpGet("change-email-pending")]
        public async Task<IActionResult> ChangeEmailPending([IgnoreText]string data)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var changeEmailModel = Encryption.DecryptModel<ChangeEmailStatusViewModel>(data);

            if (SharedBusinessLogic.TestOptions.ShowEmailVerifyLink)
            {
                // generate verify code
                var code = ChangeEmailService.CreateEmailVerificationCode(changeEmailModel.NewEmail, VirtualUser);

                // generate the verify url
                var returnVerifyUrl = ChangeEmailService.GenerateChangeEmailVerificationUrl(code);

                ViewBag.VerifyUrl = returnVerifyUrl;
            }

            return View(nameof(ChangeEmailPending), changeEmailModel);
        }

        [AllowAnonymous]
        [HttpGet("verify-change-email")]
        public async Task<IActionResult> VerifyChangeEmail([IgnoreText] string code)
        {
            // if not logged in go straight to CompleteChangeEmailAsync
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null && checkResult is ChallengeResult)
                return RedirectToAction(
                    nameof(CompleteChangeEmail),
                    new {code});

            // force sign-out then prompt sign-in before confirming email
            var redirectUrl = Url.Action<ChangeEmailController>(
                nameof(CompleteChangeEmail),
                new {code},
                "https");

            return await LogoutUser(redirectUrl);
        }

        [HttpGet("complete-change-email")]
        public async Task<IActionResult> CompleteChangeEmail([IgnoreText] string code)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            // complete email change
            var errors = await ChangeEmailService.CompleteChangeEmailAsync(code, VirtualUser);
            if (errors.ErrorCount > 0)
            {
                // show failed reason
                ModelState.Merge(errors);
                return View("ChangeEmailFailed");
            }

            // show success
            var changeEmailCompletedModel = new ChangeEmailStatusViewModel {NewEmail = VirtualUser.EmailAddress};

            return View("ChangeEmailCompleted", changeEmailCompletedModel);
        }
    }
}