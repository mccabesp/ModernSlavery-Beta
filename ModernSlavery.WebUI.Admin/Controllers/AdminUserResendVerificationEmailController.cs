using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Admin.Classes;
using ModernSlavery.WebUI.Admin.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Parsers;
using ModernSlavery.WebUI.Shared.Classes.UrlHelper;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoleNames.Admin)]
    [Route("admin")]
    public class AdminUserResendVerificationEmailController : BaseController
    {
        private readonly IAdminService _adminService;
        private readonly AuditLogger auditLogger;

        public AdminUserResendVerificationEmailController(
            IAdminService adminService,
            AuditLogger auditLogger,
            ILogger<AdminUserResendVerificationEmailController> logger, IWebService webService,
            ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            _adminService = adminService;
            this.auditLogger = auditLogger;
        }

        [HttpGet("user/{id}/resend-verification-email")]
        public IActionResult ResendVerificationEmailGet(long id)
        {
            var user = SharedBusinessLogic.DataRepository.Get<User>(id);

            var viewModel = new AdminResendVerificationEmailViewModel {User = user};

            if (user.EmailVerifiedDate != null)
                viewModel.AddErrorFor<AdminResendVerificationEmailViewModel, object>(
                    m => m.OtherErrorMessagePlaceholder,
                    "This user's email address has already been verified");

            return View("ResendVerificationEmail", viewModel);
        }

        [HttpPost("user/{id}/resend-verification-email")]
        public async Task<IActionResult> ResendVerificationEmailPost(long id,
            AdminResendVerificationEmailViewModel viewModel)
        {
            var user = SharedBusinessLogic.DataRepository.Get<User>(id);
            viewModel.User = user;

            if (user.EmailVerifiedDate != null)
            {
                viewModel.AddErrorFor<AdminResendVerificationEmailViewModel, object>(
                    m => m.OtherErrorMessagePlaceholder,
                    "This user's email address has already been verified");
                return View("ResendVerificationEmail", viewModel);
            }

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (!ModelState.IsValid)
                foreach (var state in ModelState.Where(state => state.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid))
                    foreach (var error in state.Value.Errors)
                        viewModel.AddErrorFor(state.Key, error.ErrorMessage);

            if (viewModel.HasAnyErrors()) return View("ResendVerificationEmail", viewModel);

            auditLogger.AuditChangeToUser(
                this,
                AuditedAction.AdminResendVerificationEmail,
                user,
                new
                {
                    viewModel.Reason
                }
            );

            var verifyCode = Encryption.Encrypt($"{user.UserId}:{user.Created.ToSmallDateTime()}", Encryption.Encodings.Base62);

            user.EmailVerifyHash = Crypto.GetSHA512Checksum(verifyCode);
            user.EmailVerifySendDate = VirtualDateTime.Now;
            SharedBusinessLogic.DataRepository.SaveChangesAsync().Wait();

            var verifyUrl = Url.ActionArea("VerifyEmail", "Account","Account",new {vcode = verifyCode},"https");

            if (!await _adminService.SharedBusinessLogic.SendEmailService.SendCreateAccountPendingVerificationAsync(verifyUrl, user.EmailAddress))
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