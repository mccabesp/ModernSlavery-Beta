using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Submission.Presenters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Submission.Controllers.NEW
{
    // Route:
    // "~/submission/<org-reference>/<year>/<action>/"
    // TODO - rename to submission when appropriate
    public class StatementMetadataController : BaseController
    {
        readonly IStatementMetadataPresenter SubmissionPresenter;

        public StatementMetadataController(
            IStatementMetadataPresenter submissionPresenter,
            ILogger logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic)
            : base(logger, webService, sharedBusinessLogic)
        {
            SubmissionPresenter = submissionPresenter;
        }

        #region General notes

        // I feel that the pattern will be the same for all the steps apart from the reivew
        // Is that correct?

        // Is an identfier parameter nessesary
        // is the current user enough to find the relevent submission?
        // might also require organisation and (maybe) year, any other parameters?

        // Authorization will be almost identical for all actions
        // except submit review, which might need some extra

        // organisation has URL slug - find and use

        // Year might tie to accounting date
        // Year being reported will be different to the deadline of the report

        #endregion

        #region Step 1 - Your statement

        [HttpGet("{organisationIdentifier}/{year}/your-statement")]
        public async Task<IActionResult> YourStatement(string organisationIdentifier, int year)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var result = await SubmissionPresenter.TryGetCompliance(CurrentUser, organisationIdentifier, year);

            return await GetActionResultFromQuery(result);
        }

        [HttpPost("{organisationIdentifier}/{year}/your-statement")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YourStatement(StatementMetadataViewModel submissionModel)
        {
            var result = await SubmissionPresenter.TrySaveCompliance(CurrentUser, submissionModel);

            return await GetActionResultFromSave(result, SubmissionStep.YourStatement);
        }

        [HttpPost("cancel-your-statement")]
        public async Task<IActionResult> CancelYourStatement()
        {
            await SubmissionPresenter.ClearDraftForUser();

            var next = await SubmissionPresenter.GetCancelRedirection();
            return RedirectToAction(next);
        }

        #endregion

        #region Step 2 - Compliance

        [HttpGet("{organisationIdentifier}/{year}/compliance")]
        public async Task<IActionResult> Compliance(string organisationIdentifier, int year)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var result = await SubmissionPresenter.TryGetCompliance(CurrentUser, organisationIdentifier, year);

            return await GetActionResultFromQuery(result);
        }

        [HttpPost("{organisationIdentifier}/{year}/compliance")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Compliance(StatementMetadataViewModel submissionModel)
        {
            var result = await SubmissionPresenter.TrySaveCompliance(CurrentUser, submissionModel);

            return await GetActionResultFromSave(result, SubmissionStep.Compliance);
        }

        [HttpPost("cancel-compliance")]
        public async Task<IActionResult> CancelCompliance()
        {
            await SubmissionPresenter.ClearDraftForUser();

            var next = await SubmissionPresenter.GetCancelRedirection();
            return RedirectToAction(next);
        }

        #endregion

        #region Step 3 - Your organisation

        #endregion

        #region Step 4 - Policies

        #endregion

        #region Step 5 - Supply chain risks and due diligence

        #endregion

        #region Step 6 - Training

        #endregion

        #region Step 7 - Monitoring progress

        #endregion

        #region Step 8 - Review

        [HttpGet("review")]
        public async Task<IActionResult> Review(string identifier) // an ID parameter
        {
            #region Authorization

            // Should match POST exactly!
            // user permission
            // can the user view your statement (eg role/permissions)
            // can the user edit the current statement for this organisation
            // is the statement in a state that can be edited, eg not submitted
            // any of this fails return 401 unauthorized

            #endregion

            #region Query domain/repository

            // get data 
            // identifier might not be passed
            // would current user be enough to act as an identifier? might also require organisation and year, any other parameters?
            // via presenter and passing identifier
            // presenter uses identifier (or lack thereof) to query domain
            // presenter creates VM from result of domain

            #endregion

            // return view with the vm

            throw new NotImplementedException();
        }

        [HttpPost("submit-review")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(/*View model*/ object vm) // submit the viewmodel
        {
            #region Authorization

            // Should match GET exactly!
            // user permission
            // can the user view your statement (eg role/permissions)
            // can the user edit the current statement for this organisation
            // is the statement in a state that can be edited, eg not submitted
            // any of this fails return 401 unauthorized

            #endregion

            #region Validation

            // Validate the VM, ModelState.IsValid
            // Validate state for domain
            // Any failed validation return the view and ensure error is presented

            #endregion

            #region Persistence


            #endregion

            #region Redirect

            // Redirect to next step?
            // Or to review?

            #endregion

            throw new NotImplementedException();
        }

        [HttpPost("save-review")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveReview(/*View model*/ object vm) // submit the viewmodel
        {
            #region Authorization

            // Should match GET exactly!
            // user permission
            // can the user view your statement (eg role/permissions)
            // can the user edit the current statement for this organisation
            // is the statement in a state that can be edited, eg not submitted
            // any of this fails return 401 unauthorized

            #endregion

            #region Validation

            // Validate the VM, ModelState.IsValid
            // Validate state for domain
            // Any failed validation return the view and ensure error is presented

            #endregion

            #region Persistence

            // Save to DB/file

            #endregion

            #region Redirect

            // Redirect to next step?
            // Or to review?

            #endregion

            throw new NotImplementedException();
        }

        #endregion

        #region Private methods

        private async Task<IActionResult> GetActionResultFromQuery(CustomResult<StatementMetadataViewModel> result)
        {
            if (result.Failed)
            {
                // interpret error to do correct action
                // redirect?
                // custom error?
            }

            // ensure the view has hidden fields for the non editable fields on this page
            return View(result.Result);
        }

        private async Task<IActionResult> GetActionResultFromSave(CustomResult<StatementMetadataViewModel> result, SubmissionStep step)
        {
            if (result.Failed)
            {
                ModelState.AddModelError(result.ErrorMessage);
                return View(result.ErrorRelatedObject);
            }

            // Redirect location
            var next = await SubmissionPresenter.GetNextRedirection(step);
            return RedirectToAction(next);
        }

        #endregion
    }
}
