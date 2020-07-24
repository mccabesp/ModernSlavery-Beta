﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Submission.Classes;
using ModernSlavery.WebUI.Submission.Models;

namespace ModernSlavery.WebUI.Submission.Controllers
{
    [Area("Submission")]
    [Authorize]
    [Route("Submit")]
    public partial class SubmissionController : BaseController
    {
        public delegate bool IsPageChanged(ReturnViewModel postedReturnViewModel,
            ReturnViewModel stashedReturnViewModel);

        private readonly IStatementBusinessLogic _statementBusinessLogic;
        private readonly ISubmissionService _SubmissionService;

        public SubmissionController(
            ISubmissionService submissionService,
            IStatementBusinessLogic statementBusinessLogic,
            ILogger<SubmissionController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) :
            base(logger, webService, sharedBusinessLogic)
        {
            _SubmissionService = submissionService;
            _statementBusinessLogic = statementBusinessLogic;
        }

        [Route("Init")]
        public IActionResult Init()
        {
            if (!SharedBusinessLogic.SharedOptions.IsProduction())
                Logger.LogInformation("Submit Controller Initialised");

            return new EmptyResult();
        }

        [Authorize]
        [HttpGet("~/manage-organisations")]
        public async Task<IActionResult> ManageOrganisations()
        {
            //Clear all the stashes
            ClearAllStashes();

            //Remove any previous searches from the cache
            _SubmissionService.PrivateSectorRepository.ClearSearch();

            //Reset the current reporting organisation
            ReportingOrganisation = null;

            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null && IsImpersonatingUser == false) return checkResult;

            // check if the user has accepted the privacy statement (unless admin or impersonating)
            if (!IsImpersonatingUser && !SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(base.CurrentUser))
            {
                var hasReadPrivacy = VirtualUser.AcceptedPrivacyStatement;
                if (hasReadPrivacy == null ||
                    hasReadPrivacy.Value < SharedBusinessLogic.SharedOptions.PrivacyChangedDate)
                    return RedirectToAction("PrivacyPolicy", "Shared");
            }

            //create the new view model 
            var model = VirtualUser.UserOrganisations.OrderBy(uo => uo.Organisation.OrganisationName);
            return View(nameof(ManageOrganisations), model);
        }

        [Authorize]
        [HttpGet("~/manage-organisation/{organisationIdentifier}")]
        public async Task<IActionResult> ManageOrganisation(string organisationIdentifier)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            // Decrypt org id
            long organisationId = _SubmissionService.SharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            if (organisationId == 0) return new HttpBadRequestResult($"Cannot decrypt organisation id {organisationIdentifier}");

            // Check the user has permission for this organisation
            var userOrg = VirtualUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
                return new HttpForbiddenResult(
                    $"User {VirtualUser?.EmailAddress} is not registered for organisation id {organisationId}");

            // clear the stash
            ClearStash();

            //Get the current snapshot date
            var currentReportingDeadline = SharedBusinessLogic.GetReportingDeadline(userOrg.Organisation.SectorType);

            //Make sure we have an explicit scope for last and year for organisations new to this year
            if (userOrg.PINConfirmedDate != null && userOrg.Organisation.Created >= currentReportingDeadline)
            {
                var scopeStatus = await _SubmissionService.ScopeBusinessLogic.GetLatestScopeStatusForSnapshotYearAsync(organisationId, currentReportingDeadline.Year - 1);
                if (!scopeStatus.IsAny(ScopeStatuses.InScope, ScopeStatuses.OutOfScope))
                    return RedirectToAction(nameof(ScopeController.DeclareScope), "Scope", new { organisationIdentifier });
            }

            // get any associated users for the current org
            var associatedUserOrgs = userOrg.GetAssociatedUsers().ToList();

            // build the view model
            var model = new ManageOrganisationModel
            {
                CurrentUserIdentifier = SharedBusinessLogic.Obfuscator.Obfuscate(userOrg.User.UserId),
                CurrentUserFullName = userOrg.User.Fullname,
                OrganisationName = userOrg.Organisation.OrganisationName,
                LatestAddress = userOrg.Organisation.LatestAddress.GetAddressString(Environment.NewLine),
                AssociatedUserOrgs = associatedUserOrgs,
                OrganisationIdentifier = SharedBusinessLogic.Obfuscator.Obfuscate(organisationId.ToString()),
                StatementInfoModels = _statementBusinessLogic.GetStatementInfoModelsAsync(userOrg.Organisation)
            };

            return View(model);
        }

        [HttpGet("submit/")]
        public async Task<IActionResult> Redirect()
        {
            await TrackPageViewAsync();

            return RedirectToAction("EnterCalculations");
        }
    }
}