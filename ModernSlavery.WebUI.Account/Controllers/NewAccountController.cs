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
            var lastSignupDate = await GetLastSignupDateAsync();
            var remainingTime = lastSignupDate == DateTime.MinValue
                ? TimeSpan.Zero
                : lastSignupDate.AddMinutes(SharedBusinessLogic.SharedOptions.MinSignupMinutes) - VirtualDateTime.Now;
            if (!SharedBusinessLogic.SharedOptions.SkipSpamProtection && remainingTime > TimeSpan.Zero)
                return View("CustomError",
                    WebService.ErrorViewModelFactory.Create(1125,
                        new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));

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
                var args = pendingFasttrackCodes?.SplitI(":");
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
        [SpamProtection]
        public async Task<IActionResult> AboutYou(SignUpViewModel model)
        {
            //Ensure IP address hasnt signed up recently
            var lastSignupDate = await GetLastSignupDateAsync();
            var remainingTime = lastSignupDate == DateTime.MinValue
                ? TimeSpan.Zero
                : lastSignupDate.AddMinutes(SharedBusinessLogic.SharedOptions.MinSignupMinutes) - VirtualDateTime.Now;
            if (!SharedBusinessLogic.SharedOptions.SkipSpamProtection && remainingTime > TimeSpan.Zero)
                ModelState.AddModelError(3024, null, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)});

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<SignUpViewModel>();
                return View("AboutYou", model);
            }

            //Validate the submitted fields
            if (model.Password.ContainsI("password")) AddModelError(3000, "Password");

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<SignUpViewModel>();
                return View("AboutYou", model);
            }

            //Ensure email is always lower case
            model.EmailAddress = model.EmailAddress.ToLower();

            //Check this email address isn't already assigned to another user
            var VirtualUser =
                await _accountService.UserRepository.FindByEmailAsync(model.EmailAddress, UserStatuses.New,
                    UserStatuses.Active);
            if (VirtualUser != null)
            {
                if (VirtualUser.EmailVerifySendDate != null)
                {
                    if (VirtualUser.EmailVerifiedDate != null)
                    {
                        //A registered user with this email already exists.
                        AddModelError(3001, "EmailAddress");
                        this.CleanModelErrors<SignUpViewModel>();
                        return View("AboutYou", model);
                    }

                    remainingTime =
                        VirtualUser.EmailVerifySendDate.Value.AddHours(SharedBusinessLogic.SharedOptions
                            .EmailVerificationExpiryHours)
                        - VirtualDateTime.Now;
                    if (remainingTime > TimeSpan.Zero)
                    {
                        AddModelError(3002, "EmailAddress",
                            new {remainingTime = remainingTime.ToFriendly(maxParts: 2)});
                        this.CleanModelErrors<SignUpViewModel>();
                        return View("AboutYou", model);
                    }
                }

                //Delete the previous user org if there is one
                var userOrg =
                    await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<UserOrganisation>(uo =>
                        uo.UserId == VirtualUser.UserId);
                if (userOrg != null) SharedBusinessLogic.DataRepository.Delete(userOrg);

                //If from a previous user then delete the previous user
                SharedBusinessLogic.DataRepository.Delete(VirtualUser);
            }

            //Save the submitted fields
            VirtualUser = new User();
            VirtualUser.Created = VirtualDateTime.Now;
            VirtualUser.Modified = VirtualUser.Created;
            VirtualUser.Firstname = model.FirstName;
            VirtualUser.Lastname = model.LastName;
            VirtualUser.JobTitle = model.JobTitle;
            if (model.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix))
                VirtualUser._EmailAddress = model.EmailAddress;
            else
                VirtualUser.EmailAddress = model.EmailAddress;

            if (!SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(VirtualUser))
            {
                VirtualUser.SetSetting(UserSettingKeys.AllowContact, model.AllowContact.ToString());
                VirtualUser.SetSetting(UserSettingKeys.SendUpdates, model.SendUpdates.ToString());
            }

            _accountService.UserRepository.UpdateUserPasswordUsingPBKDF2(VirtualUser, model.Password);

            VirtualUser.EmailVerifySendDate = null;
            VirtualUser.EmailVerifiedDate = null;
            VirtualUser.EmailVerifyHash = null;

            //Save the user with new status
            VirtualUser.SetStatus(UserStatuses.New, OriginalUser ?? VirtualUser);

            // save the current user
            SharedBusinessLogic.DataRepository.Insert(VirtualUser);
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            //Save pendingFasttrackCodes
            var pendingFasttrackCodes = PendingFasttrackCodes;
            if (pendingFasttrackCodes != null)
            {
                var args = pendingFasttrackCodes?.SplitI(":");
                pendingFasttrackCodes = $"{args[0]}:{args[1]}";
                VirtualUser.SetSetting(UserSettingKeys.PendingFasttrackCodes, pendingFasttrackCodes);
                await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                PendingFasttrackCodes = null;
            }

            //Send the verification code and show confirmation
            StashModel(model);

            //Ensure signup is restricted to every 10 min
            await SetLastSignupDateAsync(model.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix)
                ? DateTime.MinValue
                : VirtualDateTime.Now);

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

            //If its an administrator go to admin home
            if (SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(VirtualUser))
                return RedirectToAction("Home","Admin");

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
                var verifyCode =
                    Encryption.EncryptQuerystring(VirtualUser.UserId + ":" + VirtualUser.Created.ToSmallDateTime());
                var verifyUrl = Url.Action("VerifyEmail", "NewAccount", new {code = verifyCode}, "https");
                if (!await SharedBusinessLogic.SendEmailService.SendCreateAccountPendingVerificationAsync(verifyUrl,
                    VirtualUser.EmailAddress))
                    return null;

                VirtualUser.EmailVerifyHash = Crypto.GetSHA512Checksum(verifyCode);
                VirtualUser.EmailVerifySendDate = VirtualDateTime.Now;

                await SharedBusinessLogic.DataRepository.SaveChangesAsync();

                Logger.LogInformation(
                    $"Email verification sent: Name {VirtualUser.Fullname}, Email:{VirtualUser.EmailAddress}, IP:{UserHostAddress}");
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

        [HttpGet("~/verify-email/{code}")]
        public async Task<IActionResult> VerifyEmail(string code = null)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            if (VirtualUser != null && !VirtualUser.EmailVerifiedDate.EqualsI(null, DateTime.MinValue))
            {
                if (SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(VirtualUser))
                    return RedirectToAction("Home", "Admin");

                return RedirectToAction("EmailConfirmed");
            }

            //Make sure we are coming from EnterCalculations or the user is logged in
            var m = UnstashModel<SignUpViewModel>();
            if (m == null && VirtualUser == null) return new ChallengeResult();

            var model = new VerifyViewModel {EmailAddress = VirtualUser.EmailAddress};

            //If email not sent
            if (VirtualUser.EmailVerifySendDate.EqualsI(null, DateTime.MinValue))
            {
                var verifyCode = await SendVerifyCodeAsync(VirtualUser);
                if (string.IsNullOrWhiteSpace(verifyCode))
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1004));

                ClearStash();

                model.Sent = true;

                //If the email address is a test email then add to viewbag

                if (VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix) ||
                    SharedBusinessLogic.SharedOptions.ShowEmailVerifyLink) ViewBag.VerifyCode = verifyCode;

                //Tell them to verify email

                return View("VerifyEmail", model);
            }

            //If verification code has expired
            if (VirtualUser.EmailVerifySendDate.Value.AddHours(SharedBusinessLogic.SharedOptions
                .EmailVerificationExpiryHours) < VirtualDateTime.Now)
            {
                AddModelError(3016);

                model.Resend = true;

                //prompt user to click to request a new one
                this.CleanModelErrors<VerifyViewModel>();
                return View("VerifyEmail", model);
            }

            var remainingLock = VirtualUser.VerifyAttemptDate == null
                ? TimeSpan.Zero
                : VirtualUser.VerifyAttemptDate.Value.AddMinutes(SharedBusinessLogic.SharedOptions.LockoutMinutes) -
                  VirtualDateTime.Now;
            var remainingResend =
                VirtualUser.EmailVerifySendDate.Value.AddHours(SharedBusinessLogic.SharedOptions
                    .EmailVerificationMinResendHours)
                - VirtualDateTime.Now;

            if (string.IsNullOrEmpty(code))
            {
                if (remainingResend > TimeSpan.Zero)
                    //Prompt to check email or wait
                    return View("CustomError",
                        WebService.ErrorViewModelFactory.Create(1102,
                            new {remainingTime = remainingLock.ToFriendly(maxParts: 2)}));

                //Prompt to click resend
                model.Resend = true;
                return View("VerifyEmail", model);
            }

            //If too many wrong attempts
            if (VirtualUser.VerifyAttempts >= SharedBusinessLogic.SharedOptions.MaxEmailVerifyAttempts &&
                remainingLock > TimeSpan.Zero)
                return View("CustomError",
                    WebService.ErrorViewModelFactory.Create(1110,
                        new {remainingTime = remainingLock.ToFriendly(maxParts: 2)}));

            ActionResult result;
            if (VirtualUser.EmailVerifyHash != Crypto.GetSHA512Checksum(code))
            {
                VirtualUser.VerifyAttempts++;

                //If code min time has elapsed 
                if (remainingResend <= TimeSpan.Zero)
                {
                    model.Resend = true;
                    AddModelError(3004);

                    //Prompt user to request a new verification code
                    this.CleanModelErrors<VerifyViewModel>();
                    result = View("VerifyEmail", model);
                }
                else if (VirtualUser.VerifyAttempts >= SharedBusinessLogic.SharedOptions.MaxEmailVerifyAttempts &&
                         remainingLock > TimeSpan.Zero)
                {
                    return View("CustomError",
                        WebService.ErrorViewModelFactory.Create(1110,
                            new {remainingTime = remainingLock.ToFriendly(maxParts: 2)}));
                }
                else
                {
                    result = View("CustomError", WebService.ErrorViewModelFactory.Create(1111));
                }
            }
            else
            {
                //Set the user as verified
                VirtualUser.EmailVerifiedDate = VirtualDateTime.Now;

                //Mark the user as active
                VirtualUser.SetStatus(UserStatuses.Active, OriginalUser ?? VirtualUser, "Email verified");

                //Get any saved fasttrack codes
                PendingFasttrackCodes = VirtualUser.GetSetting(UserSettingKeys.PendingFasttrackCodes);
                VirtualUser.SetSetting(UserSettingKeys.PendingFasttrackCodes, null);

                VirtualUser.VerifyAttempts = 0;

                //If not an administrator show confirmation action to choose next step
                result = RedirectToAction("EmailConfirmed");
            }

            VirtualUser.VerifyAttemptDate = VirtualDateTime.Now;

            //Save the current user
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            //Prompt the user with confirmation
            return result;
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyViewModel model)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Reset the verification send date
            VirtualUser.EmailVerifySendDate = null;
            VirtualUser.EmailVerifyHash = null;
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            //Call GET action which will automatically resend
            return await VerifyEmail();
        }

        #endregion
    }
}