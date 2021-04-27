using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.Admin.Classes;
using ModernSlavery.WebUI.Admin.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Parsers;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoleNames.SuperOrDatabaseAdmins)]
    [Route("admin")]
    [NoCache]
    public class AdminUserContactPreferencesController : BaseController
    {
        private readonly IAdminService _adminService;
        private readonly AuditLogger auditLogger;

        public AdminUserContactPreferencesController(
            IAdminService adminService, 
            AuditLogger auditLogger,
            ILogger<AdminUserContactPreferencesController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            _adminService = adminService;
            this.auditLogger = auditLogger;
        }

        [HttpGet("user/{id}/change-contact-preferences")]
        public async Task<IActionResult> ChangeContactPreferencesGet(long id)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var user = _adminService.SharedBusinessLogic.DataRepository.Get<User>(id);

            var viewModel = new AdminChangeUserContactPreferencesViewModel
            {
                UserId = user.UserId,
                FullName = user.Fullname,
                AllowContact = user.AllowContact,
                SendUpdates = user.SendUpdates
            };

            return View("ChangeContactPreferences", viewModel);
        }

        [HttpPost("user/{id}/change-contact-preferences")]
        public async Task<IActionResult> ChangeContactPreferencesPost(long id, AdminChangeUserContactPreferencesViewModel viewModel)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var user = _adminService.SharedBusinessLogic.DataRepository.Get<User>(id);

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (!ModelState.IsValid)
                foreach (var state in ModelState.Where(state => state.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid))
                    foreach (var error in state.Value.Errors)
                        viewModel.AddErrorFor(state.Key, error.ErrorMessage);

            if (viewModel.HasAnyErrors())
            {
                viewModel.UserId = user.UserId;
                viewModel.FullName = user.Fullname;

                return View("ChangeContactPreferences", viewModel);
            }

            await auditLogger.AuditChangeToUserAsync(
                this,
                AuditedAction.AdminChangeUserContactPreferences,
                user,
                new
                {
                    AllowContact_Old = user.AllowContact ? "Yes" : "No",
                    AllowContact_New = viewModel.AllowContact ? "Yes" : "No",
                    SendUpdates_Old = user.SendUpdates ? "Yes" : "No",
                    SendUpdates_New = viewModel.SendUpdates ? "Yes" : "No",
                    viewModel.Reason
                });

            user.AllowContact = viewModel.AllowContact;
            user.SendUpdates = viewModel.SendUpdates;

            await _adminService.SharedBusinessLogic.DataRepository.SaveChangesAsync();

            return RedirectToAction("ViewUser", "AdminViewUser", new {id = user.UserId});
        }
    }
}