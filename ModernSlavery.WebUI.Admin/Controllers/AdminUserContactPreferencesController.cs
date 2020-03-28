using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Admin.Classes;
using ModernSlavery.WebUI.Admin.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Parsers;
using ModernSlavery.WebUI.Shared.Classes;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminUserContactPreferencesController : Controller
    {
        private readonly AuditLogger auditLogger;

        private readonly IDataRepository dataRepository;

        public AdminUserContactPreferencesController(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }

        [HttpGet("user/{id}/change-contact-preferences")]
        public IActionResult ChangeContactPreferencesGet(long id)
        {
            var user = dataRepository.Get<User>(id);

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
        public IActionResult ChangeContactPreferencesPost(long id, AdminChangeUserContactPreferencesViewModel viewModel)
        {
            var user = dataRepository.Get<User>(id);

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                viewModel.UserId = user.UserId;
                viewModel.FullName = user.Fullname;

                return View("ChangeContactPreferences", viewModel);
            }

            auditLogger.AuditChangeToUser(
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

            dataRepository.SaveChangesAsync().Wait();

            return RedirectToAction("ViewUser", "AdminViewUser", new {id = user.UserId});
        }
    }
}