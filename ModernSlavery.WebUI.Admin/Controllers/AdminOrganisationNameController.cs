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
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Admin.Classes;
using ModernSlavery.WebUI.Admin.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Parsers;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoleNames.BasicAdmin)]
    [Route("admin")]
    public class AdminOrganisationNameController : BaseController
    {
        private readonly AuditLogger _auditLogger;
        private readonly ICompaniesHouseAPI _companiesHouseApi;
        private readonly ISearchBusinessLogic _searchBusinessLogic;

        public AdminOrganisationNameController(
            ICompaniesHouseAPI companiesHouseApi,
            ISearchBusinessLogic searchBusinessLogic,
            AuditLogger auditLogger,
            ILogger<AdminOrganisationNameController> logger, IWebService webService,
            ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            _companiesHouseApi = companiesHouseApi;
            _searchBusinessLogic = searchBusinessLogic;
            _auditLogger = auditLogger;
        }

        [HttpGet("organisation/{id}/name")]
        public async Task<IActionResult> ViewNameHistory(long id)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var organisation = SharedBusinessLogic.DataRepository.Get<Organisation>(id);

            return View("ViewOrganisationName", organisation);
        }

        [HttpGet("organisation/{id}/name/change")]
        [Authorize(Roles = UserRoleNames.SuperOrDatabaseAdmins)]
        public async Task<IActionResult> ChangeNameGet(long id)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var organisation = SharedBusinessLogic.DataRepository.Get<Organisation>(id);

            if (!string.IsNullOrWhiteSpace(organisation.CompanyNumber))
                try
                {
                    var organisationFromCompaniesHouse = await _companiesHouseApi.GetCompanyAsync(organisation.CompanyNumber);

                    if (organisationFromCompaniesHouse != null)
                    {
                        var nameFromCompaniesHouse = organisationFromCompaniesHouse.CompanyName;

                        if (!string.Equals(organisation.OrganisationName, nameFromCompaniesHouse, StringComparison.Ordinal))
                            return OfferNewCompaniesHouseName(organisation, nameFromCompaniesHouse);
                    }
                }
                catch (Exception ex)
                {
                    // Use Manual Change page instead
                    WebService.CustomLogger.Warning("Error from Companies House API", ex);
                }

            // In all other cases...
            // * Organisation doesn't have a Companies House number
            // * CoHo API returns an error
            // * CoHo name matches Organisation name
            // ... send to the Manual Change page
            return SendToManualChangePage(organisation);
        }

        private IActionResult OfferNewCompaniesHouseName(Organisation organisation, string nameFromCompaniesHouse)
        {
            var viewModel = new ChangeOrganisationNameViewModel
            {
                Organisation = organisation,
                Action = ManuallyChangeOrganisationNameViewModelActions.OfferNewCompaniesHouseName,
                Name = nameFromCompaniesHouse
            };

            return View("OfferNewCompaniesHouseName", viewModel);
        }

        private IActionResult SendToManualChangePage(Organisation organisation)
        {
            var viewModel = new ChangeOrganisationNameViewModel
            {
                Organisation = organisation,
                Action = ManuallyChangeOrganisationNameViewModelActions.ManualChange,
                Name = organisation.OrganisationName
            };

            return View("ManuallyChangeOrganisationName", viewModel);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("organisation/{id}/name/change")]
        [Authorize(Roles = UserRoleNames.SuperOrDatabaseAdmins)]
        public async Task<IActionResult> ChangeNamePostAsync(long id, ChangeOrganisationNameViewModel viewModel)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            // We might need to change the value of Action before we go to the view
            // Apparently this is necessary
            // https://stackoverflow.com/questions/4837744/hiddenfor-not-getting-correct-value-from-view-model
            ModelState.Clear();

            var organisation = SharedBusinessLogic.DataRepository.Get<Organisation>(id);

            switch (viewModel.Action)
            {
                case ManuallyChangeOrganisationNameViewModelActions.OfferNewCompaniesHouseName:
                    return OfferNewCompaniesHouseAction(viewModel, organisation);

                case ManuallyChangeOrganisationNameViewModelActions.ManualChange:
                    return ManualChangeAction(viewModel, organisation);

                case ManuallyChangeOrganisationNameViewModelActions.CheckChangesManual:
                case ManuallyChangeOrganisationNameViewModelActions.CheckChangesCoHo:
                    return await CheckChangesActionAsync(viewModel, organisation);

                default:
                    throw new ArgumentException("Unknown action in AdminOrganisationNameController.ChangeNamePost");
            }
        }

        private IActionResult OfferNewCompaniesHouseAction(ChangeOrganisationNameViewModel viewModel,
            Organisation organisation)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.AcceptCompaniesHouseName);

            if (!ModelState.IsValid)
                foreach (var state in ModelState.Where(state => state.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid))
                    foreach (var error in state.Value.Errors)
                        viewModel.AddErrorFor(state.Key, error.ErrorMessage);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Organisation = organisation;
                viewModel.Action = ManuallyChangeOrganisationNameViewModelActions.OfferNewCompaniesHouseName;
                return View("OfferNewCompaniesHouseName", viewModel);
            }

            if (viewModel.AcceptCompaniesHouseName == AcceptCompaniesHouseName.Reject)
                return SendToManualChangePage(organisation);

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (!ModelState.IsValid)
                foreach (var state in ModelState.Where(state => state.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid))
                    foreach (var error in state.Value.Errors)
                        viewModel.AddErrorFor(state.Key, error.ErrorMessage);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Organisation = organisation;
                viewModel.Action = ManuallyChangeOrganisationNameViewModelActions.OfferNewCompaniesHouseName;
                return View("OfferNewCompaniesHouseName", viewModel);
            }

            viewModel.Organisation = organisation;
            viewModel.Action = ManuallyChangeOrganisationNameViewModelActions.CheckChangesCoHo;
            return View("ConfirmNameChange", viewModel);
        }

        private IActionResult ManualChangeAction(ChangeOrganisationNameViewModel viewModel, Organisation organisation)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.Name);
            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (!ModelState.IsValid)
                foreach (var state in ModelState.Where(state => state.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid))
                    foreach (var error in state.Value.Errors)
                        viewModel.AddErrorFor(state.Key, error.ErrorMessage);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Organisation = organisation;
                viewModel.Action = ManuallyChangeOrganisationNameViewModelActions.ManualChange;
                return View("ManuallyChangeOrganisationName", viewModel);
            }

            viewModel.Organisation = organisation;
            viewModel.Action = ManuallyChangeOrganisationNameViewModelActions.CheckChangesManual;
            return View("ConfirmNameChange", viewModel);
        }

        private async Task<IActionResult> CheckChangesActionAsync(ChangeOrganisationNameViewModel viewModel, Organisation organisation)
        {
            if (viewModel.Action == ManuallyChangeOrganisationNameViewModelActions.CheckChangesManual)
                await OptOrganisationOutOfCompaniesHouseUpdatesAsync(organisation);

            await SaveChangesAndAuditActionAsync(viewModel, organisation);

            return View("SuccessfullyChangedOrganisationName", organisation);
        }

        private async Task SaveChangesAndAuditActionAsync(ChangeOrganisationNameViewModel viewModel, Organisation organisation)
        {
            var oldName = organisation.OrganisationName;

            var newOrganisationName = CreateOrganisationNameFromViewModel(viewModel);
            AddNewNameToOrganisation(newOrganisationName, organisation);

            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            await _auditLogger.AuditChangeToOrganisationAsync(
                this,
                AuditedAction.AdminChangeOrganisationName,
                organisation,
                new
                {
                    viewModel.Action,
                    OldName = oldName,
                    NewName = newOrganisationName.Name,
                    NewNameId = newOrganisationName.OrganisationNameId,
                    viewModel.Reason
                });

            //Update the search record
            await _searchBusinessLogic.RefreshSearchDocumentsAsync(organisation);
        }

        private OrganisationName CreateOrganisationNameFromViewModel(ChangeOrganisationNameViewModel viewModel)
        {
            var organisationName = new OrganisationName
            {
                Name = viewModel.Name,
                Created = VirtualDateTime.Now,
                Source = "Service Desk"
            };

            return organisationName;
        }

        private void AddNewNameToOrganisation(OrganisationName organisationName, Organisation organisation)
        {
            organisationName.OrganisationId = organisation.OrganisationId;
            organisation.OrganisationNames.Add(organisationName);
            organisation.OrganisationName = organisationName.Name;

            SharedBusinessLogic.DataRepository.Insert(organisationName);
        }

        private async Task OptOrganisationOutOfCompaniesHouseUpdatesAsync(Organisation organisation)
        {
            organisation.OptedOutFromCompaniesHouseUpdate = true;
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();
        }
    }
}