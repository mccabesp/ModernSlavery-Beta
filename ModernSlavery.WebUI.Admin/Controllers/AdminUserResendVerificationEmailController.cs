using System.Threading.Tasks;
using ModernSlavery.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Admin.Controllers;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Options;
using ModernSlavery.WebUI.GDSDesignSystem.Parsers;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Admin.Models
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminUserResendVerificationEmailController : BaseController
    {

        private readonly AuditLogger auditLogger;
        private readonly ISendEmailService emailSender;

        public AdminUserResendVerificationEmailController(
            ISendEmailService emailSender, AuditLogger auditLogger,
            ILogger<AdminUserResendVerificationEmailController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            this.auditLogger = auditLogger;
            this.emailSender = emailSender;
        }

        [HttpGet("user/{id}/resend-verification-email")]
        public IActionResult ResendVerificationEmailGet(long id)
        {
            User user = SharedBusinessLogic.DataRepository.Get<User>(id);

            var viewModel = new AdminResendVerificationEmailViewModel { User = user };

            if (user.EmailVerifiedDate != null)
            {
                viewModel.AddErrorFor<AdminResendVerificationEmailViewModel, object>(
                    m => m.OtherErrorMessagePlaceholder,
                    "This user's email address has already been verified");
            }

            return View("ResendVerificationEmail", viewModel);
        }

        [HttpPost("user/{id}/resend-verification-email")]
        public async Task<IActionResult> ResendVerificationEmailPost(long id, AdminResendVerificationEmailViewModel viewModel)
        {
            User user = SharedBusinessLogic.DataRepository.Get<User>(id);
            viewModel.User = user;

            if (user.EmailVerifiedDate != null)
            {
                viewModel.AddErrorFor<AdminResendVerificationEmailViewModel, object>(
                    m => m.OtherErrorMessagePlaceholder, 
                    "This user's email address has already been verified");
                return View("ResendVerificationEmail", viewModel);
            }

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);
            if (viewModel.HasAnyErrors())
            {
                return View("ResendVerificationEmail", viewModel);
            }

            auditLogger.AuditChangeToUser(
                this,
                AuditedAction.AdminResendVerificationEmail,
                user,
                new
                {
                    Reason = viewModel.Reason
                }
                );

            string verifyCode = Encryption.EncryptQuerystring(user.UserId + ":" + user.Created.ToSmallDateTime());

            user.EmailVerifyHash = Crypto.GetSHA512Checksum(verifyCode);
            user.EmailVerifySendDate = VirtualDateTime.Now;
            SharedBusinessLogic.DataRepository.SaveChangesAsync().Wait();

            string verifyUrl = await WebService.RouteHelper.Get(UrlRouteOptions.Routes.AccountVerifyEmail,new { vcode=verifyCode });

            if (!emailSender.SendCreateAccountPendingVerificationAsync(verifyUrl, user.EmailAddress).Result)
            {
                viewModel.AddErrorFor<AdminResendVerificationEmailViewModel, object>(
                    m => m.OtherErrorMessagePlaceholder,
                    "Error whilst re-sending verification email. Please try again in a few minutes.");
                return View("ResendVerificationEmail", viewModel);
            }

            return View("VerificationEmailSent", user);
        }

    }
}
