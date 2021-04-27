using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Classes;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models.CompaniesHouse;
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
    public class AdminOrganisationCompaniesHouseOptInOutController : BaseController
    {
        private readonly IAdminService _adminService;
        private readonly AuditLogger auditLogger;
        private readonly ICompaniesHouseAPI companiesHouseApi;
        private readonly ICompaniesHouseService updateFromCompaniesHouseService;

        public AdminOrganisationCompaniesHouseOptInOutController(
            IAdminService adminService, 
            AuditLogger auditLogger,
            ICompaniesHouseAPI companiesHouseApi,
            ICompaniesHouseService updateFromCompaniesHouseService,
            ILogger<AdminOrganisationCompaniesHouseOptInOutController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            _adminService = adminService;
            this.auditLogger = auditLogger;
            this.companiesHouseApi = companiesHouseApi;
            this.updateFromCompaniesHouseService = updateFromCompaniesHouseService;
        }


        [HttpGet("organisation/{id}/coho-sync/opt-in")]
        public async Task<IActionResult> OptIn(long id)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var organisation = _adminService.SharedBusinessLogic.DataRepository.Get<Organisation>(id);

            var model = new AdminChangeCompaniesHouseOptInOutViewModel();
            model.Organisation = organisation;
            await PopulateViewModelWithCompanyFromCompaniesHouseAsync(model, organisation);

            return View("OptIn", model);
        }

        [HttpPost("organisation/{id}/coho-sync/opt-in")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OptIn(long id, AdminChangeCompaniesHouseOptInOutViewModel viewModel)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var organisation = _adminService.SharedBusinessLogic.DataRepository.Get<Organisation>(id);

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (!ModelState.IsValid)
                foreach (var state in ModelState.Where(state => state.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid))
                    foreach (var error in state.Value.Errors)
                        viewModel.AddErrorFor(state.Key, error.ErrorMessage);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Organisation = organisation;
                await PopulateViewModelWithCompanyFromCompaniesHouseAsync(viewModel, organisation);

                // If there are any errors, return the user back to the same page to correct the mistakes
                return View("OptIn", viewModel);
            }

            await updateFromCompaniesHouseService.UpdateOrganisationAsync(organisation);

            organisation.OptedOutFromCompaniesHouseUpdate = false;
            await _adminService.SharedBusinessLogic.DataRepository.SaveChangesAsync();

            await auditLogger.AuditChangeToOrganisationAsync(
                this,
                AuditedAction.AdminChangeCompaniesHouseOpting,
                organisation,
                new
                {
                    Opt = "In", viewModel.Reason
                });

            return RedirectToAction("ViewOrganisation", "AdminViewOrganisation",
                new {id = organisation.OrganisationId});
        }

        private async Task PopulateViewModelWithCompanyFromCompaniesHouseAsync(AdminChangeCompaniesHouseOptInOutViewModel viewModel, Organisation organisation)
        {
            var companiesHouseCompany = await companiesHouseApi.GetCompanyAsync(organisation.CompanyNumber);
            if (companiesHouseCompany==null)throw new Exception("This organisation doesn't have a companies house record.");

            viewModel.CompaniesHouseCompany = companiesHouseCompany;
        }


        [HttpGet("organisation/{id}/coho-sync/opt-out")]
        public async Task<IActionResult> OptOut(long id)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var organisation = _adminService.SharedBusinessLogic.DataRepository.Get<Organisation>(id);

            var model = new AdminChangeCompaniesHouseOptInOutViewModel();
            model.Organisation = organisation;

            return View("OptOut", model);
        }

        [HttpPost("organisation/{id}/coho-sync/opt-out")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OptOut(long id, AdminChangeCompaniesHouseOptInOutViewModel viewModel)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var organisation = _adminService.SharedBusinessLogic.DataRepository.Get<Organisation>(id);

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (!ModelState.IsValid)
                foreach (var state in ModelState.Where(state => state.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid))
                    foreach (var error in state.Value.Errors)
                        viewModel.AddErrorFor(state.Key, error.ErrorMessage);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Organisation = organisation;

                // If there are any errors, return the user back to the same page to correct the mistakes
                return View("OptOut", viewModel);
            }

            organisation.OptedOutFromCompaniesHouseUpdate = true;
            await _adminService.SharedBusinessLogic.DataRepository.SaveChangesAsync();

            await auditLogger.AuditChangeToOrganisationAsync(
                this,
                AuditedAction.AdminChangeCompaniesHouseOpting,
                organisation,
                new
                {
                    Opt = "Out", viewModel.Reason
                });

            return RedirectToAction("ViewOrganisation", "AdminViewOrganisation",
                new {id = organisation.OrganisationId});
        }
    }
}