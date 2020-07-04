using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Admin.Classes;
using ModernSlavery.WebUI.Admin.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Parsers;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminReturnLateFlagController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly AuditLogger auditLogger;

        public AdminReturnLateFlagController(
            IAdminService adminService, 
            AuditLogger auditLogger)
        {
            _adminService = adminService;
            this.auditLogger = auditLogger;
        }

        [HttpGet("return/{id}/change-late-flag")]
        public IActionResult ChangeLateFlag(long id)
        {
            var specifiedReturn = _adminService.SharedBusinessLogic.DataRepository.Get<Return>(id);

            var viewModel = new AdminReturnLateFlagViewModel
                {Return = specifiedReturn, NewLateFlag = !specifiedReturn.IsLateSubmission};

            return View("ChangeLateFlag", viewModel);
        }

        [HttpPost("return/{id}/change-late-flag")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeLateFlag(long id, AdminReturnLateFlagViewModel viewModel)
        {
            var specifiedReturn = _adminService.SharedBusinessLogic.DataRepository.Get<Return>(id);

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Return = specifiedReturn;

                // If there are any errors, return the user back to the same page to correct the mistakes
                return View("ChangeLateFlag", viewModel);
            }

            if (viewModel.NewLateFlag is null) throw new ArgumentNullException(nameof(viewModel.NewLateFlag));

            specifiedReturn.IsLateSubmission = viewModel.NewLateFlag.Value;

            _adminService.SharedBusinessLogic.DataRepository.SaveChangesAsync().Wait();

            auditLogger.AuditChangeToOrganisation(
                this,
                AuditedAction.AdminChangeLateFlag,
                specifiedReturn.Organisation,
                new
                {
                    ReturnId = id,
                    LateFlagChangedTo = viewModel.NewLateFlag,
                    viewModel.Reason
                });

            return RedirectToAction("ViewOrganisation", "AdminViewOrganisation",
                new {id = specifiedReturn.OrganisationId});
        }
    }
}