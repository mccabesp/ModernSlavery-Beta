using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
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
    public class AdminOrganisationScopeController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly AuditLogger auditLogger;

        public AdminOrganisationScopeController(
            IAdminService adminService, 
            AuditLogger auditLogger)
        {
            _adminService = adminService;
            this.auditLogger = auditLogger;
        }

        [HttpGet("organisation/{id}/scope")]
        public IActionResult ViewScopeHistory(long id)
        {
            var organisation = _adminService.SharedBusinessLogic.DataRepository.Get<Organisation>(id);

            return View("ViewOrganisationScope", organisation);
        }

        [HttpGet("organisation/{id}/scope/change/{year}")]
        public IActionResult ChangeScopeGet(long id, int year)
        {
            var organisation = _adminService.SharedBusinessLogic.DataRepository.Get<Organisation>(id);
            var currentScopeStatus = organisation.GetActiveScope(year).ScopeStatus;

            var viewModel = new AdminChangeScopeViewModel
            {
                OrganisationName = organisation.OrganisationName,
                OrganisationId = organisation.OrganisationId,
                ReportingYear = year,
                CurrentScopeStatus = currentScopeStatus,
                NewScopeStatus = GetNewScopeStatus(currentScopeStatus)
            };

            return View("ChangeScope", viewModel);
        }

        [HttpPost("organisation/{id}/scope/change/{year}")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeScopePost(long id, int year, AdminChangeScopeViewModel viewModel)
        {
            var organisation = _adminService.SharedBusinessLogic.DataRepository.Get<Organisation>(id);
            var currentOrganisationScope = organisation.GetActiveScope(year);

            if (currentOrganisationScope.ScopeStatus != ScopeStatuses.InScope
                && currentOrganisationScope.ScopeStatus != ScopeStatuses.OutOfScope)
                viewModel.ParseAndValidateParameters(Request, m => m.NewScopeStatus);

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                // If there are any errors, return the user back to the same page to correct the mistakes
                var currentScopeStatus = organisation.GetActiveScope(year).ScopeStatus;

                viewModel.OrganisationName = organisation.OrganisationName;
                viewModel.OrganisationId = organisation.OrganisationId;
                viewModel.ReportingYear = year;
                viewModel.CurrentScopeStatus = currentScopeStatus;

                return View("ChangeScope", viewModel);
            }

            RetireOldScopesForCurrentSnapshotDate(organisation, year);

            var newScope = ConvertNewScopeStatusToScopeStatus(viewModel.NewScopeStatus);

            var newOrganisationScope = new OrganisationScope
            {
                Organisation = organisation,
                ScopeStatus = newScope,
                ScopeStatusDate = VirtualDateTime.Now,
                ContactFirstname = currentOrganisationScope.ContactFirstname,
                ContactLastname = currentOrganisationScope.ContactLastname,
                ContactEmailAddress = currentOrganisationScope.ContactEmailAddress,
                Reason = viewModel.Reason,
                SubmissionDeadline = currentOrganisationScope.SubmissionDeadline,
                StatusDetails = "Changed by Admin",
                Status = ScopeRowStatuses.Active
            };

            _adminService.SharedBusinessLogic.DataRepository.Insert(newOrganisationScope);
            await _adminService.SharedBusinessLogic.DataRepository.SaveChangesAsync();

            organisation.LatestScope = newOrganisationScope;
            await _adminService.SharedBusinessLogic.DataRepository.SaveChangesAsync();

            auditLogger.AuditChangeToOrganisation(
                this,
                AuditedAction.AdminChangeOrganisationScope,
                organisation,
                new
                {
                    PreviousScope = currentOrganisationScope.ScopeStatus.ToString(),
                    NewScope = newScope.ToString(),
                    viewModel.Reason
                });

            return RedirectToAction("ViewScopeHistory", "AdminOrganisationScope",
                new {id = organisation.OrganisationId});
        }

        private void RetireOldScopesForCurrentSnapshotDate(Organisation organisation, int year)
        {
            var organisationScopesForCurrentSnapshotDate =
                organisation.OrganisationScopes
                    .Where(scope => scope.SubmissionDeadline.Year == year);

            foreach (var organisationScope in organisationScopesForCurrentSnapshotDate)
                organisationScope.Status = ScopeRowStatuses.Retired;
        }

        private NewScopeStatus? GetNewScopeStatus(ScopeStatuses previousScopeStatus)
        {
            if (previousScopeStatus == ScopeStatuses.InScope) return NewScopeStatus.OutOfScope;

            if (previousScopeStatus == ScopeStatuses.OutOfScope) return NewScopeStatus.InScope;

            return null;
        }

        private ScopeStatuses ConvertNewScopeStatusToScopeStatus(NewScopeStatus? newScopeStatus)
        {
            if (newScopeStatus == NewScopeStatus.InScope) return ScopeStatuses.InScope;

            if (newScopeStatus == NewScopeStatus.OutOfScope) return ScopeStatuses.OutOfScope;

            throw new Exception();
        }
    }
}