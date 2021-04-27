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
    public class AdminOrganisationAddressController : BaseController
    {
        private readonly AuditLogger _auditLogger;
        private readonly ICompaniesHouseAPI _companiesHouseApi;
        private readonly ICompaniesHouseService _updateFromCompaniesHouseService;
        private readonly ISearchBusinessLogic _searchBusinessLogic;

        public AdminOrganisationAddressController(
            ICompaniesHouseAPI companiesHouseApi,
            ICompaniesHouseService updateFromCompaniesHouseService,
            ISearchBusinessLogic searchBusinessLogic,
            AuditLogger auditLogger,
            ILogger<AdminOrganisationAddressController> logger,
            IWebService webService,
            ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            _updateFromCompaniesHouseService = updateFromCompaniesHouseService;
            _auditLogger = auditLogger;
            _searchBusinessLogic = searchBusinessLogic;
            _companiesHouseApi = companiesHouseApi;
        }

        [HttpGet("organisation/{id}/address")]
        public async Task<IActionResult> ViewAddressHistory(long id)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var organisation = SharedBusinessLogic.DataRepository.Get<Organisation>(id);

            return View("ViewOrganisationAddress", organisation);
        }

        [HttpGet("organisation/{id}/address/change")]
        [Authorize(Roles = UserRoleNames.SuperOrDatabaseAdmins)]
        public async Task<IActionResult> ChangeAddressGet(long id)
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
                        var addressFromCompaniesHouse = await _updateFromCompaniesHouseService.CreateOrganisationAddressFromCompaniesHouseAddressAsync(organisationFromCompaniesHouse.RegisteredOfficeAddress);

                        if (!organisation.GetLatestAddress().AddressMatches(addressFromCompaniesHouse))
                            return OfferNewCompaniesHouseAddress(organisation, addressFromCompaniesHouse);
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
            // * CoHo address matches Organisation address
            // ... send to the Manual Change page
            return SendToManualChangePage(organisation);
        }

        private IActionResult OfferNewCompaniesHouseAddress(Organisation organisation,
            OrganisationAddress addressFromCompaniesHouse)
        {
            var viewModel = new ChangeOrganisationAddressViewModel
            {
                Organisation = organisation,
                Action = ManuallyChangeOrganisationAddressViewModelActions.OfferNewCompaniesHouseAddress
            };

            viewModel.PopulateFromOrganisationAddress(addressFromCompaniesHouse);

            return View("OfferNewCompaniesHouseAddress", viewModel);
        }

        private IActionResult SendToManualChangePage(Organisation organisation)
        {
            var address = organisation.OrganisationAddresses.OrderByDescending(a => a.Created).First();

            var viewModel = new ChangeOrganisationAddressViewModel
            {
                Organisation = organisation,
                Action = ManuallyChangeOrganisationAddressViewModelActions.ManualChange
            };

            viewModel.PopulateFromOrganisationAddress(address);

            return View("ManuallyChangeOrganisationAddress", viewModel);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("organisation/{id}/address/change")]
        [Authorize(Roles = UserRoleNames.SuperOrDatabaseAdmins)]
        public async Task<IActionResult> ChangeAddressPost(long id, ChangeOrganisationAddressViewModel viewModel)
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
                case ManuallyChangeOrganisationAddressViewModelActions.OfferNewCompaniesHouseAddress:
                    return OfferNewCompaniesHouseAction(viewModel, organisation);

                case ManuallyChangeOrganisationAddressViewModelActions.ManualChange:
                    return ManualChangeAction(viewModel, organisation);

                case ManuallyChangeOrganisationAddressViewModelActions.CheckChangesManual:
                case ManuallyChangeOrganisationAddressViewModelActions.CheckChangesCoHo:
                    return await CheckChangesActionAsync(viewModel, organisation);

                default:
                    throw new ArgumentException(
                        "Unknown action in AdminOrganisationAddressController.ChangeAddressPost");
            }
        }

        private IActionResult OfferNewCompaniesHouseAction(ChangeOrganisationAddressViewModel viewModel,
            Organisation organisation)
        {            
            viewModel.ParseAndValidateParameters(Request, m => m.AcceptCompaniesHouseAddress);

            if (!ModelState.IsValid)
                foreach (var state in ModelState.Where(state => state.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid))
                    foreach (var error in state.Value.Errors)
                        viewModel.AddErrorFor(state.Key, error.ErrorMessage);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Organisation = organisation;
                viewModel.Action = ManuallyChangeOrganisationAddressViewModelActions.OfferNewCompaniesHouseAddress;
                return View("OfferNewCompaniesHouseAddress", viewModel);
            }

            if (viewModel.AcceptCompaniesHouseAddress == AcceptCompaniesHouseAddress.Reject)
                return SendToManualChangePage(organisation);

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (!ModelState.IsValid)
                foreach (var state in ModelState.Where(state => state.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid))
                    foreach (var error in state.Value.Errors)
                        viewModel.AddErrorFor(state.Key, error.ErrorMessage);

            if (!ModelState.IsValid)
                foreach (var state in ModelState.Where(state => state.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid))
                    foreach (var error in state.Value.Errors)
                        viewModel.AddErrorFor(state.Key, error.ErrorMessage);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Organisation = organisation;
                viewModel.Action = ManuallyChangeOrganisationAddressViewModelActions.OfferNewCompaniesHouseAddress;
                return View("OfferNewCompaniesHouseAddress", viewModel);
            }

            viewModel.Organisation = organisation;
            viewModel.Action = ManuallyChangeOrganisationAddressViewModelActions.CheckChangesCoHo;
            return View("ConfirmAddressChange", viewModel);
        }

        private IActionResult ManualChangeAction(ChangeOrganisationAddressViewModel viewModel,
            Organisation organisation)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.PoBox);
            viewModel.ParseAndValidateParameters(Request, m => m.Address1);
            viewModel.ParseAndValidateParameters(Request, m => m.Address2);
            viewModel.ParseAndValidateParameters(Request, m => m.Address3);
            viewModel.ParseAndValidateParameters(Request, m => m.TownCity);
            viewModel.ParseAndValidateParameters(Request, m => m.County);
            viewModel.ParseAndValidateParameters(Request, m => m.Country);
            viewModel.ParseAndValidateParameters(Request, m => m.PostCode);
            viewModel.ParseAndValidateParameters(Request, m => m.IsUkAddress);
            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (!ModelState.IsValid)
                foreach (var state in ModelState.Where(state => state.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid))
                    foreach (var error in state.Value.Errors)
                        viewModel.AddErrorFor(state.Key, error.ErrorMessage);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Organisation = organisation;
                viewModel.Action = ManuallyChangeOrganisationAddressViewModelActions.ManualChange;
                return View("ManuallyChangeOrganisationAddress", viewModel);
            }

            viewModel.Organisation = organisation;
            viewModel.Action = ManuallyChangeOrganisationAddressViewModelActions.CheckChangesManual;
            return View("ConfirmAddressChange", viewModel);
        }

        private async Task<IActionResult> CheckChangesActionAsync(ChangeOrganisationAddressViewModel viewModel,
            Organisation organisation)
        {
            if (viewModel.Action == ManuallyChangeOrganisationAddressViewModelActions.CheckChangesManual)
                await OptOrganisationOutOfCompaniesHouseUpdatesAsync(organisation);

            await SaveChangesAndAuditActionAsync(viewModel, organisation);

            return View("SuccessfullyChangedOrganisationAddress", organisation);
        }

        private async Task SaveChangesAndAuditActionAsync(ChangeOrganisationAddressViewModel viewModel, Organisation organisation)
        {
            var oldAddressString = organisation.GetLatestAddress()?.GetAddressString();

            RetireOldAddress(organisation);

            var newOrganisationAddress = CreateOrganisationAddressFromViewModel(viewModel);
            AddNewAddressToOrganisation(newOrganisationAddress, organisation);

            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            await _auditLogger.AuditChangeToOrganisationAsync(
                this,
                AuditedAction.AdminChangeOrganisationAddress,
                organisation,
                new
                {
                    viewModel.Action,
                    OldAddress = oldAddressString,
                    NewAddress = newOrganisationAddress.GetAddressString(),
                    NewAddressId = newOrganisationAddress.AddressId,
                    viewModel.Reason
                });

            //Update the search record
            await _searchBusinessLogic.RefreshSearchDocumentsAsync(organisation);
        }

        private static void RetireOldAddress(Organisation organisation)
        {
            var oldOrganisationAddress = organisation.GetLatestAddress();
            oldOrganisationAddress.Status = AddressStatuses.Retired;
            oldOrganisationAddress.StatusDate = VirtualDateTime.Now;
            oldOrganisationAddress.Modified = VirtualDateTime.Now;
        }

        private OrganisationAddress CreateOrganisationAddressFromViewModel(ChangeOrganisationAddressViewModel viewModel)
        {
            var organisationAddress = new OrganisationAddress
            {
                Address1 = viewModel.Address1,
                Address2 = viewModel.Address2,
                Address3 = viewModel.Address3,
                TownCity = viewModel.TownCity,
                County = viewModel.County,
                Country = viewModel.Country,
                PostCode = viewModel.PostCode,
                PoBox = viewModel.PoBox,
                Status = AddressStatuses.Active,
                StatusDate = VirtualDateTime.Now,
                StatusDetails = viewModel.Reason,
                Modified = VirtualDateTime.Now,
                Created = VirtualDateTime.Now,
                Source = "Service Desk"
            };

            organisationAddress.Trim();

            if (viewModel.IsUkAddress.HasValue)
                organisationAddress.IsUkAddress =
                    viewModel.IsUkAddress == ManuallyChangeOrganisationAddressIsUkAddress.Yes;

            return organisationAddress;
        }

        private void AddNewAddressToOrganisation(OrganisationAddress organisationAddress, Organisation organisation)
        {
            organisationAddress.OrganisationId = organisation.OrganisationId;
            organisation.OrganisationAddresses.Add(organisationAddress);
            organisation.LatestAddress = organisationAddress;

            SharedBusinessLogic.DataRepository.Insert(organisationAddress);
        }

        private async Task OptOrganisationOutOfCompaniesHouseUpdatesAsync(Organisation organisation)
        {
            organisation.OptedOutFromCompaniesHouseUpdate = true;
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();
        }
    }
}