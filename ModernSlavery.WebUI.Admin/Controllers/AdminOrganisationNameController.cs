using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Admin.Classes;
using ModernSlavery.WebUI.Admin.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Parsers;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminOrganisationNameController : BaseController
    {
        private readonly AuditLogger auditLogger;
        private readonly ICompaniesHouseAPI companiesHouseApi;

        public AdminOrganisationNameController(
            ICompaniesHouseAPI companiesHouseApi,
            AuditLogger auditLogger,
            ILogger<AdminOrganisationNameController> logger, IWebService webService,
            ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            this.companiesHouseApi = companiesHouseApi;
            this.auditLogger = auditLogger;
        }

        [HttpGet("organisation/{id}/name")]
        public IActionResult ViewNameHistory(long id)
        {
            var organisation = SharedBusinessLogic.DataRepository.Get<Organisation>(id);

            return View("ViewOrganisationName", organisation);
        }

        [HttpGet("organisation/{id}/name/change")]
        public IActionResult ChangeNameGet(long id)
        {
            var organisation = SharedBusinessLogic.DataRepository.Get<Organisation>(id);

            if (!string.IsNullOrWhiteSpace(organisation.CompanyNumber))
                try
                {
                    var organisationFromCompaniesHouse =
                        companiesHouseApi.GetCompanyAsync(organisation.CompanyNumber).Result;

                    var nameFromCompaniesHouse = organisationFromCompaniesHouse.CompanyName;

                    if (!string.Equals(organisation.OrganisationName, nameFromCompaniesHouse, StringComparison.Ordinal))
                        return OfferNewCompaniesHouseName(organisation, nameFromCompaniesHouse);
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
        public IActionResult ChangeNamePost(long id, ChangeOrganisationNameViewModel viewModel)
        {
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
                    return CheckChangesAction(viewModel, organisation);

                default:
                    throw new ArgumentException("Unknown action in AdminOrganisationNameController.ChangeNamePost");
            }
        }

        private IActionResult OfferNewCompaniesHouseAction(ChangeOrganisationNameViewModel viewModel,
            Organisation organisation)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.AcceptCompaniesHouseName);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Organisation = organisation;
                viewModel.Action = ManuallyChangeOrganisationNameViewModelActions.OfferNewCompaniesHouseName;
                return View("OfferNewCompaniesHouseName", viewModel);
            }

            if (viewModel.AcceptCompaniesHouseName == AcceptCompaniesHouseName.Reject)
                return SendToManualChangePage(organisation);

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

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

        private IActionResult CheckChangesAction(ChangeOrganisationNameViewModel viewModel, Organisation organisation)
        {
            if (viewModel.Action == ManuallyChangeOrganisationNameViewModelActions.CheckChangesManual)
                OptOrganisationOutOfCompaniesHouseUpdates(organisation);

            SaveChangesAndAuditAction(viewModel, organisation);

            return View("SuccessfullyChangedOrganisationName", organisation);
        }

        private void SaveChangesAndAuditAction(ChangeOrganisationNameViewModel viewModel, Organisation organisation)
        {
            var oldName = organisation.OrganisationName;

            var newOrganisationName = CreateOrganisationNameFromViewModel(viewModel);
            AddNewNameToOrganisation(newOrganisationName, organisation);

            SharedBusinessLogic.DataRepository.SaveChangesAsync().Wait();

            auditLogger.AuditChangeToOrganisation(
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

        private void OptOrganisationOutOfCompaniesHouseUpdates(Organisation organisation)
        {
            organisation.OptedOutFromCompaniesHouseUpdate = true;
            SharedBusinessLogic.DataRepository.SaveChangesAsync().Wait();
        }
    }
}