using System;
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
    public class AdminReturnLateFlagController : BaseController
    {
        private readonly IAdminService _adminService;
        private readonly AuditLogger auditLogger;

        public AdminReturnLateFlagController(
            IAdminService adminService, 
            AuditLogger auditLogger,
            ILogger<AdminReturnLateFlagController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            _adminService = adminService;
            this.auditLogger = auditLogger;
        }

        [HttpGet("return/{id}/change-late-flag")]
        public async Task<IActionResult> ChangeLateFlag(long id)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var specifiedStatement = _adminService.SharedBusinessLogic.DataRepository.Get<Statement>(id);

            var viewModel = new AdminStatementLateFlagViewModel
                {Statement = specifiedStatement, NewLateFlag = !specifiedStatement.IsLateSubmission};

            return View("ChangeLateFlag", viewModel);
        }

        [HttpPost("return/{id}/change-late-flag")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeLateFlag(long id, AdminStatementLateFlagViewModel viewModel)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var specifiedStatement = _adminService.SharedBusinessLogic.DataRepository.Get<Statement>(id);

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (!ModelState.IsValid)
                foreach (var state in ModelState.Where(state => state.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid))
                    foreach (var error in state.Value.Errors)
                        viewModel.AddErrorFor(state.Key, error.ErrorMessage);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Statement = specifiedStatement;

                // If there are any errors, return the user back to the same page to correct the mistakes
                return View("ChangeLateFlag", viewModel);
            }

            if (viewModel.NewLateFlag is null) throw new ArgumentNullException(nameof(viewModel.NewLateFlag));

            specifiedStatement.IsLateSubmission = viewModel.NewLateFlag.Value;

            await _adminService.SharedBusinessLogic.DataRepository.SaveChangesAsync();

            await auditLogger.AuditChangeToOrganisationAsync(
                this,
                AuditedAction.AdminChangeLateFlag,
                specifiedStatement.Organisation,
                new
                {
                    ReturnId = id,
                    LateFlagChangedTo = viewModel.NewLateFlag,
                    viewModel.Reason
                });

            return RedirectToAction("ViewOrganisation", "AdminViewOrganisation",
                new {id = specifiedStatement.OrganisationId});
        }
    }
}