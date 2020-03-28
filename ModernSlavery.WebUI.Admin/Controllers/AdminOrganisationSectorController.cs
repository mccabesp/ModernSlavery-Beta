using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Admin.Classes;
using ModernSlavery.WebUI.Admin.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Parsers;
using ModernSlavery.WebUI.Shared.Classes;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminOrganisationSectorController : Controller
    {
        private readonly AuditLogger auditLogger;
        private readonly IDataRepository dataRepository;

        public AdminOrganisationSectorController(
            IDataRepository dataRepository,
            AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }

        [HttpGet("organisation/{id}/change-public-sector-classification")]
        public IActionResult ChangePublicSectorClassificationGet(long id)
        {
            var organisation = dataRepository.Get<Organisation>(id);

            var viewModel = new AdminChangePublicSectorClassificationViewModel
            {
                OrganisationId = organisation.OrganisationId,
                OrganisationName = organisation.OrganisationName,
                PublicSectorTypes = dataRepository.GetAll<PublicSectorType>().ToList(),
                SelectedPublicSectorTypeId = organisation.LatestPublicSectorType?.PublicSectorTypeId
            };

            return View("ChangePublicSectorClassification", viewModel);
        }

        [HttpPost("organisation/{id}/change-public-sector-classification")]
        public IActionResult ChangePublicSectorClassificationPost(long id,
            AdminChangePublicSectorClassificationViewModel viewModel)
        {
            var organisation = dataRepository.Get<Organisation>(id);
            viewModel.OrganisationId = organisation.OrganisationId;
            viewModel.OrganisationName = organisation.OrganisationName;
            viewModel.PublicSectorTypes = dataRepository.GetAll<PublicSectorType>().ToList();

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (!viewModel.SelectedPublicSectorTypeId.HasValue)
                viewModel.AddErrorFor<AdminChangePublicSectorClassificationViewModel, int?>(
                    m => m.SelectedPublicSectorTypeId,
                    "Please select a public sector classification");

            if (viewModel.HasAnyErrors()) return View("ChangePublicSectorClassification", viewModel);

            var newPublicSectorType = dataRepository.GetAll<PublicSectorType>()
                .FirstOrDefault(p => p.PublicSectorTypeId == viewModel.SelectedPublicSectorTypeId.Value);
            if (newPublicSectorType == null)
                throw new ArgumentException(
                    $"User selected an invalid PublicSectorType ({viewModel.SelectedPublicSectorTypeId})");

            AuditChange(viewModel, organisation, newPublicSectorType);

            RetireExistingOrganisationPublicSectorTypesForOrganisation(organisation);

            AddNewOrganisationPublicSectorType(organisation, viewModel.SelectedPublicSectorTypeId.Value);

            dataRepository.SaveChangesAsync().Wait();

            return RedirectToAction("ViewOrganisation", "AdminViewOrganisation",
                new {id = organisation.OrganisationId});
        }

        private void AuditChange(
            AdminChangePublicSectorClassificationViewModel viewModel,
            Organisation organisation,
            PublicSectorType newPublicSectorType)
        {
            auditLogger.AuditChangeToOrganisation(
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
            var organisationPublicSectorTypes = dataRepository.GetAll<OrganisationPublicSectorType>()
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

            dataRepository.Insert(newOrganisationPublicSectorType);

            organisation.LatestPublicSectorType = newOrganisationPublicSectorType;
        }
    }
}