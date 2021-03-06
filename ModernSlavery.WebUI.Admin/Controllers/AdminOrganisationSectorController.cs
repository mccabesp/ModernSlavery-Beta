﻿using System;
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
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoleNames.SuperOrDatabaseAdmins)]
    [Route("admin")]
    [NoCache]
    public class AdminOrganisationSectorController : BaseController
    {
        private readonly IAdminService _adminService;
        private readonly AuditLogger auditLogger;

        public AdminOrganisationSectorController(
            IAdminService adminService,
            AuditLogger auditLogger,
            ILogger<AdminOrganisationSectorController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            _adminService = adminService;
            this.auditLogger = auditLogger;
        }

        [HttpGet("organisation/{id}/change-public-sector-classification")]
        public async Task<IActionResult> ChangePublicSectorClassificationGet(long id)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var organisation = _adminService.SharedBusinessLogic.DataRepository.Get<Organisation>(id);

            var viewModel = new AdminChangePublicSectorClassificationViewModel
            {
                OrganisationId = organisation.OrganisationId,
                OrganisationName = organisation.OrganisationName,
                PublicSectorTypes = _adminService.SharedBusinessLogic.DataRepository.GetAll<PublicSectorType>().ToList(),
                SelectedPublicSectorTypeId = organisation.LatestPublicSectorType?.PublicSectorTypeId
            };

            return View("ChangePublicSectorClassification", viewModel);
        }

        [HttpPost("organisation/{id}/change-public-sector-classification")]
        public async Task<IActionResult> ChangePublicSectorClassificationPost(long id,
            AdminChangePublicSectorClassificationViewModel viewModel)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var organisation = _adminService.SharedBusinessLogic.DataRepository.Get<Organisation>(id);
            viewModel.OrganisationId = organisation.OrganisationId;
            viewModel.OrganisationName = organisation.OrganisationName;
            viewModel.PublicSectorTypes = _adminService.SharedBusinessLogic.DataRepository.GetAll<PublicSectorType>().ToList();

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (!viewModel.SelectedPublicSectorTypeId.HasValue)
                viewModel.AddErrorFor<AdminChangePublicSectorClassificationViewModel, int?>(
                    m => m.SelectedPublicSectorTypeId,
                    "Please select a public sector classification");

            if (!ModelState.IsValid)
                foreach (var state in ModelState.Where(state => state.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid))
                    foreach (var error in state.Value.Errors)
                        viewModel.AddErrorFor(state.Key, error.ErrorMessage);

            if (viewModel.HasAnyErrors()) return View("ChangePublicSectorClassification", viewModel);

            var newPublicSectorType = _adminService.SharedBusinessLogic.DataRepository.GetAll<PublicSectorType>()
                .FirstOrDefault(p => p.PublicSectorTypeId == viewModel.SelectedPublicSectorTypeId.Value);
            if (newPublicSectorType == null)
                throw new ArgumentException(
                    $"User selected an invalid PublicSectorType ({viewModel.SelectedPublicSectorTypeId})");

            await AuditChangeAsync(viewModel, organisation, newPublicSectorType);

            RetireExistingOrganisationPublicSectorTypesForOrganisation(organisation);

            AddNewOrganisationPublicSectorType(organisation, viewModel.SelectedPublicSectorTypeId.Value);

            await _adminService.SharedBusinessLogic.DataRepository.SaveChangesAsync();

            return RedirectToAction("ViewOrganisation", "AdminViewOrganisation",
                new {id = organisation.OrganisationId});
        }

        private async Task AuditChangeAsync(
            AdminChangePublicSectorClassificationViewModel viewModel,
            Organisation organisation,
            PublicSectorType newPublicSectorType)
        {
            await auditLogger.AuditChangeToOrganisationAsync(
                this,
                AuditedAction.AdminChangeOrganisationPublicSectorClassification,
                organisation,
                new
                {
                    OldClassification = organisation.LatestPublicSectorType?.PublicSectorType?.Description,
                    NewClassification = newPublicSectorType.Description,
                    viewModel.Reason
                });
        }

        private void RetireExistingOrganisationPublicSectorTypesForOrganisation(Organisation organisation)
        {
            var organisationPublicSectorTypes = _adminService.SharedBusinessLogic.DataRepository.GetAll<OrganisationPublicSectorType>()
                .Where(opst => opst.OrganisationId == organisation.OrganisationId)
                .ToList();

            foreach (var organisationPublicSectorType in organisationPublicSectorTypes)
                organisationPublicSectorType.Retired = VirtualDateTime.Now;
        }

        private void AddNewOrganisationPublicSectorType(Organisation organisation, int publicSectorTypeId)
        {
            var newOrganisationPublicSectorType = new OrganisationPublicSectorType
            {
                OrganisationId = organisation.OrganisationId,
                PublicSectorTypeId = publicSectorTypeId,
                Source = "Service Desk"
            };

            _adminService.SharedBusinessLogic.DataRepository.Insert(newOrganisationPublicSectorType);

            organisation.LatestPublicSectorType = newOrganisationPublicSectorType;
        }
    }
}