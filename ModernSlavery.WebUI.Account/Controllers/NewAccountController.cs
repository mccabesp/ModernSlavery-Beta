using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
using ModernSlavery.WebUI.Shared.Options;

namespace ModernSlavery.WebUI.Account.Controllers
{
    [Area("Account")]
    [Route("sign-up")]
    public class NewAccountController : BaseController
    {
        private readonly IAccountService _accountService;

        public NewAccountController(
            IAccountService accountService,
            ILogger<AccountController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(
            logger, webService, sharedBusinessLogic)
        {
            _accountService = accountService;
        }

        private async Task<DateTime> GetLastSignupDateAsync()
        {
            return await Cache.GetAsync<DateTime>($"{UserHostAddress}:LastSignupDate");
        }

        private async Task SetLastSignupDateAsync(DateTime value)
        {
            await Cache.RemoveAsync($"{UserHostAddress}:LastSignupDate");
            if (value > DateTime.MinValue)
                await Cache.AddAsync($"{UserHostAddress}:LastSignupDate", value,
                    value.AddMinutes(SharedBusinessLogic.SharedOptions.MinSignupMinutes));
        }

        [HttpGet]
        public async Task<IActionResult> Redirect()
        {
            await TrackPageViewAsync();

            return RedirectToActionPermanent("AboutYou");
        }

        [HttpGet("about-you")]
        public async Task<IActionResult> AboutYou()
        {
            if (!SharedBusinessLogic.TestOptions.DisableLockoutProtection)
            {
                var lastSignupDate = await GetLastSignupDateAsync();
                var remainingTime = lastSignupDate == DateTime.MinValue
                    ? TimeSpan.Zero
                    : lastSignupDate.AddMinutes(SharedBusinessLogic.SharedOptions.MinSignupMinutes) - VirtualDateTime.Now;
                if (remainingTime > TimeSpan.Zero)
                    return View("CustomError",
                        WebService.ErrorViewModelFactory.Create(1125,
                            new { remainingTime = remainingTime.ToFriendly(maxParts: 2) }));
            }

            //Ensure user has not completed the registration process
            var result = await CheckUserRegisteredOkAsync();
            if (result != null) return result;

            //Clear the stash
            ClearStash();

            var model = new SignUpViewModel();

            //Prepopulate with name and email saved during out-of-scope process
            var pendingFasttrackCodes = PendingFasttrackCodes;
            if (pendingFasttrackCodes != null)
            {
                var args = pendingFasttrackCodes?.SplitI(':');
                if (args.Length > 2) model.FirstName = args[2];

                if (args.Length > 3) model.LastName = args[3];

                if (args.Length > 4) model.EmailAddress = args[4];

                model.ConfirmEmailAddress = model.EmailAddress;
            }

            //Start new user signup
            return View("AboutYou", model);
        }

        [HttpPost("about-you")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [BotProtection]
        public async Task<IActionResult> AboutYou(SignUpViewModel model)
        {
            //Ensure IP address hasnt signed up recently
            if (!SharedBusinessLogic.TestOptions.DisableLockoutProtection)
            {
                var lastSignupDate = await GetLastSignupDateAsync();
                var remainingTime = lastSignupDate == DateTime.MinValue
                    ? TimeSpan.Zero
                    : lastSignupDate.AddMinutes(SharedBusinessLogic.SharedOptions.MinSignupMinutes) - VirtualDateTime.Now;
                if (remainingTime > TimeSpan.Zero)
                    ModelState.AddModelError(3024, null, new { remainingTime = remainingTime.ToFriendly(maxParts: 2) });
            }

            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors<SignUpViewModel>();
                return View("AboutYou", model);
            }

            //Only allow whitelisted email addresses to signup
            if (!SharedBusinessLogic.TestOptions.IsWhitelistUser(model.EmailAddress))
            {
                ModelState.AddModelError(1157, nameof(model.EmailAddress));
                return View("AboutYou", model);
            }

            //Validate the submitted fields
            if (model.Password.ContainsI("password")) AddModelError(3000, "Password");

            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors<SignUpViewModel>();
                return View("AboutYou", model);
            }

            //Ensure email is always lower case
            model.EmailAddress = model.EmailAddress.ToLower();

            //Check this email address isn't already assigned to another user
            var virtualUser = await _accountService.UserRepository.FindByEmailAsync(model.EmailAddress, UserStatuses.New, UserStatuses.Active);
            if (virtualUser != null)
            {
                //A registered user with this email already exists.
                AddModelError(3001, "EmailAddress");
                this.SetModelCustomErrors<SignUpViewModel>();
                return View("AboutYou", model);
            }

            //Save the submitted fields
            virtualUser = new User();
            virtualUser.Created = VirtualDateTime.Now;
            virtualUser.Modified = virtualUser.Created;
            virtualUser.Firstname = model.FirstName;
            virtualUser.Lastname = model.LastName;
            virtualUser.JobTitle = model.JobTitle;
            virtualUser.EmailAddress = model.EmailAddress;

            if (!SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(virtualUser))
            {
                virtualUser.SetSetting(UserSettingKeys.AllowContact, model.AllowContact.ToString());
                virtualUser.SetSetting(UserSettingKeys.SendUpdates, model.SendUpdates.ToString());
            }

            await _accountService.UserRepository.UpdateUserPasswordUsingPBKDF2Async(virtualUser, model.Password);

            virtualUser.EmailVerifySendDate = null;
            virtualUser.EmailVerifiedDate = null;
            virtualUser.EmailVerifyHash = null;

            //Save the user with new status
            virtualUser.SetStatus(UserStatuses.New, OriginalUser ?? virtualUser);

            // save the current user
            SharedBusinessLogic.DataRepository.Insert(virtualUser);
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            Logger.LogInformation($"New user created: Name {virtualUser.Fullname}, Email:{virtualUser.EmailAddress}, IP:{UserHostAddress}");

            //Save pendingFasttrackCodes
            var pendingFasttrackCodes = PendingFasttrackCodes;
            if (pendingFasttrackCodes != null)
            {
                var args = pendingFasttrackCodes?.SplitI(':');
                pendingFasttrackCodes = $"{args[0]}:{args[1]}";
                virtualUser.SetSetting(UserSettingKeys.PendingFasttrackCodes, pendingFasttrackCodes);
                await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                PendingFasttrackCodes = null;
            }

            //Send the verification code and show confirmation
            StashModel(model);

            //Ensure signup is restricted to every 10 min
            if (!SharedBusinessLogic.TestOptions.DisableLockoutProtection)
                await SetLastSignupDateAsync(VirtualDateTime.Now);

            return RedirectToAction("VerifyEmail");
        }

        #region EmailConfirmed

        [Authorize]
        [HttpGet("email-confirmed")]
        public async Task<IActionResult> EmailConfirmed()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            return View("EmailConfirmed");
        }

        #endregion

        #region Verify email

        //Send the verification code and show confirmation
        private async Task<string> SendVerifyCodeAsync(User VirtualUser)
        {
            //Send a verification link to the email address
            try
            {
                var verifyCode = Encryption.Encrypt($"{VirtualUser.UserId}:{VirtualUser.Created.ToSmallDateTime()}", Encryption.Encodings.Base62);
                var verifyUrl = Url.Action("VerifyEmail", "NewAccount", new { code = verifyCode }, "https");
                if (!await SharedBusinessLogic.SendEmailService.SendCreateAccountPendingVerificationAsync(verifyUrl,VirtualUser.EmailAddress))
                    return null;

                VirtualUser.EmailVerifyHash = Crypto.GetSHA512Checksum(verifyCode);
                VirtualUser.EmailVerifySendDate = VirtualDateTime.Now;

                await SharedBusinessLogic.DataRepository.SaveChangesAsync();

                Logger.LogInformation($"Email verification queued: Name {VirtualUser.Fullname}, Email:{VirtualUser.EmailAddress}, IP:{UserHostAddress}");
                return verifyCode;
            }
            catch (Exception ex)
            {
                //Log the exception
                Logger.LogError(ex, ex.Message);
            }

            //Prompt user to open email and verification link
            return null;
        }

        [HttpGet("verify-email/{code?}")]
        public async Task<IActionResult> VerifyEmail([IgnoreText] string code = null)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            #region Get the user trying to sign up or a signed in user
            var m = UnstashModel<SignUpViewModel>();
            var virtualUser = VirtualUser ?? (m != null ? await _accountService.UserRepository.FindByEmailAsync(m.EmailAddress, UserStatuses.New, UserStatuses.Active) : null);

            //Ensure user is signed in unless coming from Signup page
            if (virtualUser == null) return new ChallengeResult();
            #endregion

            #region Check if the email is already verified
            if (!virtualUser.EmailVerifiedDate.EqualsI(null, DateTime.MinValue))
            {
                if (SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(virtualUser))
                    return RedirectToActionArea("Home", "Admin", "Admin");

                return RedirectToAction("EmailConfirmed");
            }
            #endregion

            #region Send verify email for 1st time
            var model = new VerifyViewModel { EmailAddress = virtualUser.EmailAddress };
            if (virtualUser.EmailVerifySendDate.EqualsI(null, DateTime.MinValue))
            {
                var verifyCode = await SendVerifyCodeAsync(virtualUser);
                if (string.IsNullOrWhiteSpace(verifyCode))
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1004));

                ClearStash();

                model.Sent = true;

                //If the email address is a test email then add to viewbag

                if (SharedBusinessLogic.TestOptions.ShowEmailVerifyLink) ViewBag.VerifyCode = verifyCode;

                //Tell them to verify email

                return View("VerifyEmail", model);
            }
            #endregion

            #region Resend verify email after 1st email expired
            if (virtualUser.EmailVerifySendDate.Value.AddHours(SharedBusinessLogic.SharedOptions
                .EmailVerificationExpiryHours) < VirtualDateTime.Now)
            {
                AddModelError(3016);

                model.Resend = true;

                //prompt user to click to request a new one
                this.SetModelCustomErrors<VerifyViewModel>();
                return View("VerifyEmail", model);
            }
            #endregion

            #region When user signs in rather than using verify link tell then to wait for email or resend email
            var remainingLock = virtualUser.VerifyAttemptDate == null
                ? TimeSpan.Zero
                : virtualUser.VerifyAttemptDate.Value.AddMinutes(SharedBusinessLogic.SharedOptions.LockoutMinutes) -
                  VirtualDateTime.Now;
            var remainingResend =
                virtualUser.EmailVerifySendDate.Value.AddHours(SharedBusinessLogic.SharedOptions
                    .EmailVerificationMinResendHours)
                - VirtualDateTime.Now;

            if (string.IsNullOrWhiteSpace(code))
            {
                if (remainingResend > TimeSpan.Zero)
                    //Prompt to check email or wait
                    return View("CustomError",
                        WebService.ErrorViewModelFactory.Create(1102,
                            new { remainingTime = remainingLock.ToFriendly(maxParts: 2) }));

                //Prompt to click resend
                model.Resend = true;
                return View("VerifyEmail", model);
            }
            #endregion

            //Always ensure user is signed in
            if (VirtualUser == null) return new ChallengeResult();

            #region Check the verification code is correct
            //If too many wrong attempts
            if (virtualUser.VerifyAttempts >= SharedBusinessLogic.SharedOptions.MaxEmailVerifyAttempts &&
                remainingLock > TimeSpan.Zero)
                return View("CustomError",
                    WebService.ErrorViewModelFactory.Create(1110,
                        new { remainingTime = remainingLock.ToFriendly(maxParts: 2) }));

            ActionResult result;
            if (virtualUser.EmailVerifyHash != Crypto.GetSHA512Checksum(code))
            {
                virtualUser.VerifyAttempts++;
                Logger.LogInformation($"New user email verification fail {virtualUser.VerifyAttempts}: Name {virtualUser.Fullname}, Email:{virtualUser.EmailAddress}, IP:{UserHostAddress}");

                //If code min time has elapsed 
                if (remainingResend <= TimeSpan.Zero)
                {
                    model.Resend = true;
                    AddModelError(3004);

                    //Prompt user to request a new verification code
                    this.SetModelCustomErrors<VerifyViewModel>();
                    result = View("VerifyEmail", model);
                }
                else if (virtualUser.VerifyAttempts >= SharedBusinessLogic.SharedOptions.MaxEmailVerifyAttempts &&
                         remainingLock > TimeSpan.Zero)
                {
                    return View("CustomError",
                        WebService.ErrorViewModelFactory.Create(1110,
                            new { remainingTime = remainingLock.ToFriendly(maxParts: 2) }));
                }
                else
                {
                    result = View("CustomError", WebService.ErrorViewModelFactory.Create(1111));
                }
            }
            else
            {
                //Set the user as verified
                virtualUser.EmailVerifiedDate = VirtualDateTime.Now;
                Logger.LogInformation($"New user email verified: Name {virtualUser.Fullname}, Email:{virtualUser.EmailAddress}, IP:{UserHostAddress}");

                //Mark the user as active
                virtualUser.SetStatus(UserStatuses.Active, OriginalUser ?? virtualUser, "Email verified");

                //Get any saved fasttrack codes
                PendingFasttrackCodes = virtualUser.GetSetting(UserSettingKeys.PendingFasttrackCodes);
                virtualUser.SetSetting(UserSettingKeys.PendingFasttrackCodes, null);

                virtualUser.VerifyAttempts = 0;

                //If not an administrator show confirmation action to choose next step
                result = RedirectToAction("EmailConfirmed");
            }

            virtualUser.VerifyAttemptDate = VirtualDateTime.Now;

            //Save the current user
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            //Prompt the user with confirmation
            return result;
            #endregion
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("verify-email/{code?}")]
        public async Task<IActionResult> VerifyEmailPost([IgnoreText] string code = null)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Reset the verification send date
            VirtualUser.EmailVerifySendDate = null;
            VirtualUser.EmailVerifyHash = null;
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            //Call GET action which will automatically resend
            return await VerifyEmail(code);
        }

        #endregion
    }
}