using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Registration.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;

namespace ModernSlavery.WebUI.Registration.Controllers
{
    public partial class RegistrationController : BaseController
    {

        #region EmailConfirmed

        [Authorize]
        [HttpGet("email-confirmed")]
        public IActionResult EmailConfirmed()
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //If its an administrator go to admin home
            if (currentUser.IsAdministrator())
            {
                return RedirectToAction("Home", "Admin", new { area = "Admin" });
            }

            return View("EmailConfirmed");
        }

        #endregion

        #region Session & Cache Properties

        #endregion

        #region Verify email

        //Send the verification code and show confirmation
        private async Task<string> SendVerifyCodeAsync(User currentUser)
        {
            //Send a verification link to the email address
            try
            {
                string verifyCode = Encryption.EncryptQuerystring(currentUser.UserId + ":" + currentUser.Created.ToSmallDateTime());
                string verifyUrl = Url.Action("VerifyEmail",null, new { code = verifyCode }, "https");
                if (!await RegistrationService.SharedBusinessLogic.SendEmailService.SendCreateAccountPendingVerificationAsync(verifyUrl, currentUser.EmailAddress))
                    return null;

                currentUser.EmailVerifyHash = Crypto.GetSHA512Checksum(verifyCode);
                currentUser.EmailVerifySendDate = VirtualDateTime.Now;

                await SharedBusinessLogic.DataRepository.SaveChangesAsync();

                Logger.LogInformation(
                    $"Email verification sent: Name {currentUser.Fullname}, Email:{currentUser.EmailAddress}, IP:{UserHostAddress}");
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

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string code = null)
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (currentUser != null && !currentUser.EmailVerifiedDate.EqualsI(null, DateTime.MinValue))
            {
                if (currentUser.IsAdministrator())
                {
                    return RedirectToAction("Home", "Admin", new { area = "Admin" });
                }

                return RedirectToAction("EmailConfirmed");
            }

            //Make sure we are coming from EnterCalculations or the user is logged in
            var m = this.UnstashModel<RegisterViewModel>();
            if (m == null && currentUser == null)
            {
                return new ChallengeResult();
            }

            if (currentUser == null)
            {
                currentUser = await RegistrationService.UserRepository.FindByEmailAsync(m.EmailAddress, UserStatuses.New, UserStatuses.Active);
            }

            var model = new VerifyViewModel {EmailAddress = currentUser.EmailAddress};

            //If email not sent
            if (currentUser.EmailVerifySendDate.EqualsI(null, DateTime.MinValue))
            {
                string verifyCode = await SendVerifyCodeAsync(currentUser);
                if (string.IsNullOrWhiteSpace(verifyCode))
                {
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1004));
                }

                this.ClearStash();

                model.Sent = true;

                //If the email address is a test email then add to viewbag

                if (currentUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix) || SharedBusinessLogic.SharedOptions.ShowEmailVerifyLink)
                {
                    ViewBag.VerifyCode = verifyCode;
                }

                //Tell them to verify email

                return View("VerifyEmail", model);
            }

            //If verification code has expired
            if (currentUser.EmailVerifySendDate.Value.AddHours(SharedBusinessLogic.SharedOptions.EmailVerificationExpiryHours) < VirtualDateTime.Now)
            {
                AddModelError(3016);

                model.Resend = true;

                //prompt user to click to request a new one
                this.CleanModelErrors<VerifyViewModel>();
                return View("VerifyEmail", model);
            }

            TimeSpan remainingLock = currentUser.VerifyAttemptDate == null
                ? TimeSpan.Zero
                : currentUser.VerifyAttemptDate.Value.AddMinutes(SharedBusinessLogic.SharedOptions.LockoutMinutes) - VirtualDateTime.Now;
            TimeSpan remainingResend = currentUser.EmailVerifySendDate.Value.AddHours(SharedBusinessLogic.SharedOptions.EmailVerificationMinResendHours)
                                       - VirtualDateTime.Now;

            if (string.IsNullOrEmpty(code))
            {
                if (remainingResend > TimeSpan.Zero)
                    //Prompt to check email or wait
                {
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1102, new {remainingTime = remainingLock.ToFriendly(maxParts: 2)}));
                }

                //Prompt to click resend
                model.Resend = true;
                return View("VerifyEmail", model);
            }

            //If too many wrong attempts
            if (currentUser.VerifyAttempts >= SharedBusinessLogic.SharedOptions.MaxEmailVerifyAttempts && remainingLock > TimeSpan.Zero)
            {
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1110, new {remainingTime = remainingLock.ToFriendly(maxParts: 2)}));
            }

            ActionResult result;
            if (currentUser.EmailVerifyHash != Crypto.GetSHA512Checksum(code))
            {
                currentUser.VerifyAttempts++;

                //If code min time has elapsed 
                if (remainingResend <= TimeSpan.Zero)
                {
                    model.Resend = true;
                    AddModelError(3004);

                    //Prompt user to request a new verification code
                    this.CleanModelErrors<VerifyViewModel>();
                    result = View("VerifyEmail", model);
                }
                else if (currentUser.VerifyAttempts >= SharedBusinessLogic.SharedOptions.MaxEmailVerifyAttempts && remainingLock > TimeSpan.Zero)
                {
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1110, new {remainingTime = remainingLock.ToFriendly(maxParts: 2)}));
                }
                else
                {
                    result = View("CustomError", WebService.ErrorViewModelFactory.Create(1111));
                }
            }
            else
            {
                //Set the user as verified
                currentUser.EmailVerifiedDate = VirtualDateTime.Now;

                //Mark the user as active
                currentUser.SetStatus(UserStatuses.Active, OriginalUser ?? currentUser, "Email verified");

                //Get any saved fasttrack codes
                PendingFasttrackCodes = currentUser.GetSetting(UserSettingKeys.PendingFasttrackCodes);
                currentUser.SetSetting(UserSettingKeys.PendingFasttrackCodes, null);

                currentUser.VerifyAttempts = 0;

                //If not an administrator show confirmation action to choose next step
                result = RedirectToAction("EmailConfirmed");
            }

            currentUser.VerifyAttemptDate = VirtualDateTime.Now;

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
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Reset the verification send date
            currentUser.EmailVerifySendDate = null;
            currentUser.EmailVerifyHash = null;
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            //Call GET action which will automatically resend
            return await VerifyEmail();
        }

        #endregion

    }
}
