using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models.CompaniesHouse;
using ModernSlavery.WebUI.Admin.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Parsers;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminOrganisationCompaniesHouseOptInOutController : Controller
    {
        private readonly AuditLogger auditLogger;
        private readonly ICompaniesHouseAPI companiesHouseApi;
        private readonly IDataRepository dataRepository;
        private readonly UpdateFromCompaniesHouseService updateFromCompaniesHouseService;

        public AdminOrganisationCompaniesHouseOptInOutController(IDataRepository dataRepository,
            AuditLogger auditLogger,
            ICompaniesHouseAPI companiesHouseApi,
            UpdateFromCompaniesHouseService updateFromCompaniesHouseService)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
            this.companiesHouseApi = companiesHouseApi;
            this.updateFromCompaniesHouseService = updateFromCompaniesHouseService;
        }


        [HttpGet("organisation/{id}/coho-sync/opt-in")]
        public IActionResult OptIn(long id)
        {
            var organisation = dataRepository.Get<Organisation>(id);

            var model = new AdminChangeCompaniesHouseOptInOutViewModel();
            model.Organisation = organisation;
            PopulateViewModelWithCompanyFromCompaniesHouse(model, organisation);

            return View("OptIn", model);
        }

        [HttpPost("organisation/{id}/coho-sync/opt-in")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public IActionResult OptIn(long id, AdminChangeCompaniesHouseOptInOutViewModel viewModel)
        {
            var organisation = dataRepository.Get<Organisation>(id);

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Organisation = organisation;
                PopulateViewModelWithCompanyFromCompaniesHouse(viewModel, organisation);

                // If there are any errors, return the user back to the same page to correct the mistakes
                return View("OptIn", viewModel);
            }

            updateFromCompaniesHouseService.UpdateOrganisationDetails(organisation.OrganisationId);

            organisation.OptedOutFromCompaniesHouseUpdate = false;
            dataRepository.SaveChangesAsync().Wait();

            auditLogger.AuditChangeToOrganisation(
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

        private void PopulateViewModelWithCompanyFromCompaniesHouse(
            AdminChangeCompaniesHouseOptInOutViewModel viewModel, Organisation organisation)
        {
            CompaniesHouseCompany companiesHouseCompany;
            try
            {
                companiesHouseCompany = companiesHouseApi.GetCompanyAsync(organisation.CompanyNumber).Result;
            }
            catch (Exception)
            {
                throw new Exception("This organisation doesn't have a companies house record.");
            }

            viewModel.CompaniesHouseCompany = companiesHouseCompany;
        }


        [HttpGet("organisation/{id}/coho-sync/opt-out")]
        public IActionResult OptOut(long id)
        {
            var organisation = dataRepository.Get<Organisation>(id);

            var model = new AdminChangeCompaniesHouseOptInOutViewModel();
            model.Organisation = organisation;

            return View("OptOut", model);
        }

        [HttpPost("organisation/{id}/coho-sync/opt-out")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public IActionResult OptOut(long id, AdminChangeCompaniesHouseOptInOutViewModel viewModel)
        {
            var organisation = dataRepository.Get<Organisation>(id);

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Organisation = organisation;

                // If there are any errors, return the user back to the same page to correct the mistakes
                return View("OptOut", viewModel);
            }

            organisation.OptedOutFromCompaniesHouseUpdate = true;
            dataRepository.SaveChangesAsync().Wait();

            auditLogger.AuditChangeToOrganisation(
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