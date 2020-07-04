using System;
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
            if (!User.Identity.IsAuthenticated) return View("SignOut");

            var returnUrl = Url.Action("SignOut", "Account", null, "https");

            return await LogoutUser(returnUrl);
        }


        [HttpGet]
        public async Task<IActionResult> ManageAccount()
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null && IsImpersonatingUser == false) return checkResult;

            // map the user to the view model
            var model = AutoMapper.Map<ManageAccountViewModel>(VirtualUser);

            // check if we have any successful changes
            if (TempData.ContainsKey(nameof(AccountResources.ChangeDetailsSuccessAlert)))
                ViewBag.ChangeSuccessMessage = AccountResources.ChangeDetailsSuccessAlert;
            else if (TempData.ContainsKey(nameof(AccountResources.ChangePasswordSuccessAlert)))
                ViewBag.ChangeSuccessMessage = AccountResources.ChangePasswordSuccessAlert;

            // generate flow urls
            ViewBag.CloseAccountUrl = "";
            ViewBag.ChangeEmailUrl = "";
            ViewBag.ChangePasswordUrl = "";
            ViewBag.ChangeDetailsUrl = "";

            if (IsImpersonatingUser == false)
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
            //Ensure IP address hasnt signed up recently
            var lastPasswordResetDate = await GetLastPasswordResetDateAsync();
            var remainingTime = lastPasswordResetDate == DateTime.MinValue
                ? TimeSpan.Zero
                : lastPasswordResetDate.AddMinutes(SharedBusinessLogic.SharedOptions.MinPasswordResetMinutes) -
                  VirtualDateTime.Now;
            if (!SharedBusinessLogic.SharedOptions.SkipSpamProtection && remainingTime > TimeSpan.Zero)
                return View("CustomError",
                    WebService.ErrorViewModelFactory.Create(1133,
                        new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));

            //Ensure user has not completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Clear the stash
            ClearStash();

            //Start new password reset
            return View("PasswordReset", new ResetViewModel());
        }

        [SpamProtection(5)]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("password-reset")]
        public async Task<IActionResult> PasswordReset(ResetViewModel model)
        {
            ModelState.Remove(nameof(model.Password));
            ModelState.Remove(nameof(model.ConfirmPassword));

            //Ensure IP address hasnt signed up recently
            var lastPasswordResetDate = await GetLastPasswordResetDateAsync();
            var remainingTime = lastPasswordResetDate == DateTime.MinValue
                ? TimeSpan.Zero
                : lastPasswordResetDate.AddMinutes(SharedBusinessLogic.SharedOptions.MinPasswordResetMinutes) -
                  VirtualDateTime.Now;
            if (!SharedBusinessLogic.SharedOptions.SkipSpamProtection && remainingTime > TimeSpan.Zero)
                ModelState.AddModelError(3026, null, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)});

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ResetViewModel>();
                return View("PasswordReset", model);
            }

            //Ensure user has not completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Validate the submitted fields
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ResetViewModel>();
                return View("PasswordReset", model);
            }

            //Ensure email is always lower case
            model.EmailAddress = model.EmailAddress.ToLower();
            ViewBag.EmailAddress = model.EmailAddress;

            //Ensure signup is restricted to every 10 mins
            await SetLastPasswordResetDateAsync(
                model.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix)
                    ? DateTime.MinValue
                    : VirtualDateTime.Now);

            // find the latest active user by email
            var user = await _accountService.UserRepository.FindByEmailAsync(model.EmailAddress, UserStatuses.Active);
            if (user == null)
            {
                Logger.LogWarning(
                    "Password reset requested for unknown email address",
                    $"Email:{model.EmailAddress}, IP:{UserHostAddress}");
                return View("PasswordResetSent");
            }

            if (!await ResendPasswordResetAsync(user))
            {
                AddModelError(1122);
                this.CleanModelErrors<ResetViewModel>();
                return View("PasswordReset", model);
            }

            VirtualUser.ResetAttempts = 0;
            VirtualUser.ResetSendDate = VirtualDateTime.Now;
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            //show confirmation
            ViewBag.EmailAddress = VirtualUser.EmailAddress;
            if (VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix))
                ViewBag.TestUrl = Url.Action(
                    "NewPassword",
                    "Account",
                    new
                    {
                        code = Encryption.EncryptQuerystring(
                            VirtualUser.UserId + ":" + VirtualDateTime.Now.ToSmallDateTime())
                    },
                    "https");

            return View("PasswordResetSent");
        }

        private async Task<bool> ResendPasswordResetAsync(User VirtualUser)
        {
            //Send a password reset link to the email address
            string resetCode = null;
            try
            {
                resetCode = Encryption.EncryptQuerystring(VirtualUser.UserId + ":" +
                                                          VirtualDateTime.Now.ToSmallDateTime());
                var resetUrl = Url.Action("NewPassword", "Account", new {code = resetCode}, "https");
                if (!await SharedBusinessLogic.SendEmailService.SendResetPasswordNotificationAsync(resetUrl,
                    VirtualUser.EmailAddress))
                    return false;

                Logger.LogInformation(
                    "Password reset sent",
                    $"Name {VirtualUser.Fullname}, Email:{VirtualUser.EmailAddress}, IP:{UserHostAddress}");
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
        public async Task<IActionResult> NewPassword(string code = null)
        {
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
            try
            {
                code = Encryption.DecryptQuerystring(code);
                code = HttpUtility.UrlDecode(code);
                var args = code.SplitI(":");
                if (args.Length != 2) throw new ArgumentException("Too few parameters in password reset code");

                userId = args[0].ToLong();
                if (userId == 0) throw new ArgumentException("Invalid user id in password reset code");

                resetDate = args[1].FromSmallDateTime();
                if (resetDate == DateTime.MinValue)
                    throw new ArgumentException("Invalid password reset date in password reset code");
            }
            catch
            {
                return (View("CustomError", WebService.ErrorViewModelFactory.Create(1123)), null);
            }

            //Get the user oganisation
            var user = SharedBusinessLogic.DataRepository.Get<User>(userId);

            if (user == null) return (View("CustomError", WebService.ErrorViewModelFactory.Create(1124)), null);

            if (resetDate.AddDays(1) < VirtualDateTime.Now)
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
                this.CleanModelErrors<ResetViewModel>();
                return View("NewPassword", model);
            }

            var m = UnstashModel<ResetViewModel>();
            if (m == null || string.IsNullOrWhiteSpace(m.Resetcode))
                return View("CustomError", WebService.ErrorViewModelFactory.Create(0));

            var passwordResult = UnwrapPasswordReset(m.Resetcode);
            if (passwordResult.Result != null) return passwordResult.Result;

            ClearStash();

            //Save the user to ensure UserId>0 for new status
            _accountService.UserRepository.UpdateUserPasswordUsingPBKDF2(passwordResult.User, model.Password);

            VirtualUser.ResetAttempts = 0;
            VirtualUser.ResetSendDate = null;
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            //Send completed notification email
            await SharedBusinessLogic.SendEmailService.SendResetPasswordCompletedAsync(passwordResult.User
                .EmailAddress);

            //Send the verification code and showconfirmation
            return View("CustomError", WebService.ErrorViewModelFactory.Create(1127));
        }

        #endregion
    }
}