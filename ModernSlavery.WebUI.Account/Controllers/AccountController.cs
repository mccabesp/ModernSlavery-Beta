﻿using System;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Account.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.UrlHelper;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Resources;

namespace ModernSlavery.WebUI.Account.Controllers
{
    [Area("Account")]
    [Route("manage-account")]
    public class AccountController : BaseController
    {
        private readonly IAccountService _accountService;

        public AccountController(
            IAccountService accountService,
            ILogger<AccountController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(
            logger, webService, sharedBusinessLogic)
        {
            _accountService = accountService;
        }

        protected async Task<DateTime> GetLastPasswordResetDateAsync()
        {
            return await Cache.GetAsync<DateTime>($"{UserHostAddress}:LastPasswordResetDate");
        }

        protected async Task SetLastPasswordResetDateAsync(DateTime value)
        {
            await Cache.RemoveAsync($"{UserHostAddress}:LastPasswordResetDate");
            if (value > DateTime.MinValue)
                await Cache.AddAsync($"{UserHostAddress}:LastPasswordResetDate", value,
                    value.AddMinutes(SharedBusinessLogic.SharedOptions.MinPasswordResetMinutes));
        }

        [HttpGet("~/sign-out")]
        public async Task<IActionResult> SignOut()
        {
            //Dont save in history
            SkipSaveHistory = true;

            if (!User.Identity.IsAuthenticated)
            {
                Session.Clear();
                return View("SignedOut");
            }

            var returnUrl = Url.Action("SignOut", "Account", null, "https");
            return await LogoutUser(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> ManageAccount()
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null && !IsImpersonatingUser) return checkResult;

            // map the user to the view model
            var model = AutoMapper.Map<ManageAccountViewModel>(VirtualUser);

            // generate flow urls
            ViewBag.CloseAccountUrl = "";
            ViewBag.ChangeEmailUrl = "";
            ViewBag.ChangePasswordUrl = "";
            ViewBag.ChangeDetailsUrl = "";

            if (!IsImpersonatingUser)
            {
                ViewBag.CloseAccountUrl =
                    Url.Action<CloseAccountController>(nameof(CloseAccountController.CloseAccount));
                ViewBag.ChangeEmailUrl = Url.Action<ChangeEmailController>(nameof(ChangeEmailController.ChangeEmail));
                ViewBag.ChangePasswordUrl =
                    Url.Action<ChangePasswordController>(nameof(ChangePasswordController.ChangePassword));
                ViewBag.ChangeDetailsUrl =
                    Url.Action<ChangeDetailsController>(nameof(ChangeDetailsController.ChangeDetails));
            }

            // remove any change updates
            TempData.Remove(nameof(AccountResources.ChangeDetailsSuccessAlert));
            TempData.Remove(nameof(AccountResources.ChangePasswordSuccessAlert));

            return View(model);
        }

        #region password-reset

        [HttpGet("password-reset")]
        public async Task<IActionResult> PasswordReset()
        {
            //Dont save in history
            SkipSaveHistory = true;

            //Ensure IP address hasnt signed up recently
            if (!SharedBusinessLogic.TestOptions.DisableLockoutProtection)
            {
                var lastPasswordResetDate = await GetLastPasswordResetDateAsync();
                var remainingTime = lastPasswordResetDate == DateTime.MinValue
                    ? TimeSpan.Zero
                    : lastPasswordResetDate.AddMinutes(SharedBusinessLogic.SharedOptions.MinPasswordResetMinutes) -
                      VirtualDateTime.Now;

                if (remainingTime > TimeSpan.Zero)
                    return View("CustomError",
                        WebService.ErrorViewModelFactory.Create(1133,
                            new { remainingTime = remainingTime.ToFriendly(maxParts: 2) }));
            }

            //Ensure user has not completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Clear the stash
            ClearStash();

            //Start new password reset
            return View("../NewAccount/PasswordReset", new ResetViewModel());
        }

        [BotProtection(5)]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("password-reset")]
        public async Task<IActionResult> PasswordReset(ResetViewModel model)
        {
            ModelState.Remove(nameof(model.Password));
            ModelState.Remove(nameof(model.ConfirmPassword));

            //Ensure IP address hasnt signed up recently
            if (!SharedBusinessLogic.TestOptions.DisableLockoutProtection)
            {
                var lastPasswordResetDate = await GetLastPasswordResetDateAsync();
                var remainingTime = lastPasswordResetDate == DateTime.MinValue
                    ? TimeSpan.Zero
                    : lastPasswordResetDate.AddMinutes(SharedBusinessLogic.SharedOptions.MinPasswordResetMinutes) -
                      VirtualDateTime.Now;
                if (remainingTime > TimeSpan.Zero)
                    ModelState.AddModelError(3026, null, new { remainingTime = remainingTime.ToFriendly(maxParts: 2) });
            }

            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors<ResetViewModel>();
                return View("../NewAccount/PasswordReset", model);
            }

            //Ensure user has not completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Validate the submitted fields
            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors<ResetViewModel>();
                return View("../NewAccount/PasswordReset", model);
            }

            //Ensure email is always lower case
            model.EmailAddress = model.EmailAddress.ToLower();
            ViewBag.EmailAddress = model.EmailAddress;

            //Ensure signup is restricted to every 10 mins
            if (!SharedBusinessLogic.TestOptions.DisableLockoutProtection)
                await SetLastPasswordResetDateAsync(VirtualDateTime.Now);

            // find the latest active user by email
            var user = await _accountService.UserRepository.FindByEmailAsync(model.EmailAddress, UserStatuses.Active, UserStatuses.New);
            if (user == null)
            {
                Logger.LogWarning("Password reset requested for unknown email address", $"Email:{model.EmailAddress}, IP:{UserHostAddress}");
                return View("PasswordResetSent");
            }

            if (!await ResendPasswordResetAsync(user))
            {
                AddModelError(1122);
                this.SetModelCustomErrors<ResetViewModel>();
                return View("../NewAccount/PasswordReset", model);
            }

            user.ResetAttempts = 0;
            user.ResetSendDate = VirtualDateTime.Now;
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            //show confirmation
            ViewBag.EmailAddress = user.EmailAddress;
            return View("PasswordResetSent");
        }

        private async Task<bool> ResendPasswordResetAsync(User VirtualUser)
        {
            try
            {
                //Send a password reset link to the email address
                var resetCode = Encryption.Encrypt($"{VirtualUser.UserId}:{VirtualDateTime.Now.ToSmallDateTime()}:{VirtualUser.PasswordHash.GetDeterministicHashCode()}", Encryption.Encodings.Base62);
                var resetUrl = Url.Action("NewPassword", "Account", new { code = resetCode }, "https");
                if (!await SharedBusinessLogic.SendEmailService.SendResetPasswordNotificationAsync(resetUrl, VirtualUser.EmailAddress))
                    return false;

                Logger.LogInformation($"Password reset sent. Name:{VirtualUser.Fullname}, Email:{VirtualUser.EmailAddress}, IP:{UserHostAddress}");
            }
            catch (Exception ex)
            {
                //Log the exception
                Logger.LogError(ex, ex.Message);
                return false;
            }

            return true;
        }

        [HttpGet("enter-new-password")]
        public async Task<IActionResult> NewPassword([IgnoreText] string code = null)
        {
            //Dont save in history
            SkipSaveHistory = true;

            //Ensure user has not completed the registration process
            var result = await CheckUserRegisteredOkAsync();
            if (result != null) return result;

            var passwordResult = UnwrapPasswordReset(code);
            if (passwordResult.Result != null) return passwordResult.Result;

            var model = new ResetViewModel();
            model.Resetcode = code;
            StashModel(model);

            //Start new user registration
            return View("NewPassword", model);
        }

        private (ActionResult Result, User User) UnwrapPasswordReset(string code)
        {
            long userId = 0;
            DateTime resetDate;

            int passwordHash = 0;
            try
            {
                code = Encryption.Decrypt(code, Encryption.Encodings.Base62);
                code = HttpUtility.UrlDecode(code);
                var args = code.SplitI(':');
                if (args.Length != 3) throw new ArgumentException("Too few parameters in password reset code");

                userId = args[0].ToLong();
                if (userId == 0) throw new ArgumentException("Invalid user id in password reset code");

                resetDate = args[1].FromSmallDateTime();
                if (resetDate == DateTime.MinValue)
                    throw new ArgumentException("Invalid password reset date in password reset code");

                passwordHash = args[2].ToInt32();
                if (passwordHash == 0)
                    throw new ArgumentException("Invalid current password hash in password reset code");
            }
            catch
            {
                return (View("CustomError", WebService.ErrorViewModelFactory.Create(1123)), null);
            }

            //Get the user oganisation
            var user = SharedBusinessLogic.DataRepository.Get<User>(userId);

            if (user == null) return (View("CustomError", WebService.ErrorViewModelFactory.Create(1124)), null);

            //Password reset after password change or expiry period
            if (resetDate.AddHours(SharedBusinessLogic.SharedOptions.PasswordResetExpiryHours) < VirtualDateTime.Now || passwordHash != user.PasswordHash.GetDeterministicHashCode())
                return (View("CustomError", WebService.ErrorViewModelFactory.Create(1126)), null);

            return (null, user);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("enter-new-password")]
        public async Task<IActionResult> NewPassword(ResetViewModel model)
        {
            //Ensure user has not completed the registration process
            var result = await CheckUserRegisteredOkAsync();
            if (result != null) return result;

            ModelState.Remove(nameof(model.EmailAddress));
            ModelState.Remove(nameof(model.ConfirmEmailAddress));

            //Validate the submitted fields
            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors<ResetViewModel>();
                return View("NewPassword", model);
            }

            var m = UnstashModel<ResetViewModel>();
            if (m == null || string.IsNullOrWhiteSpace(m.Resetcode))
                return View("CustomError", WebService.ErrorViewModelFactory.Create(0));

            var passwordResult = UnwrapPasswordReset(m.Resetcode);
            if (passwordResult.Result != null) return passwordResult.Result;

            ClearStash();

            //Save the user to ensure UserId>0 for new status
            await _accountService.UserRepository.UpdateUserPasswordUsingPBKDF2Async(passwordResult.User, model.Password);

            passwordResult.User.ResetAttempts = 0;
            passwordResult.User.ResetSendDate = null;
            passwordResult.User.LoginAttempts = 0;
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            //Send completed notification email
            await SharedBusinessLogic.SendEmailService.SendResetPasswordCompletedAsync(passwordResult.User.EmailAddress);

            //Send the verification code and showconfirmation
            return View("CustomError", WebService.ErrorViewModelFactory.Create(1127));
        }

        #endregion
    }
}