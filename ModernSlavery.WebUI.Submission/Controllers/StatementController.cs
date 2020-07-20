using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Submission.Presenters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Submission.Controllers
{
    [Area("Submission")]
    [Route("statement")]
    public class StatementController : BaseController
    {
        readonly IStatementPresenter SubmissionPresenter;

        public StatementController(
            IStatementPresenter submissionPresenter,
            ILogger<StatementController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic)
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

        #region Before You Start
        [HttpGet("{organisationIdentifier}/{year}/before-you-start")]
        public async Task<IActionResult> BeforeYouStart(string organisationIdentifier, int year)
        {
            var result = SubmissionPresenter.CheckStatementAccess(this.VirtualUser, organisationIdentifier, year);
            if (result==)
            return View(Url.Action("YourStatement",new { OrganisationIdentifier = organisationIdentifier, Year = year }));
        }

        #endregion

        #region Step 1 - Your statementyou

        [HttpGet("{organisationIdentifier}/{year}/your-statement")]
        public async Task<IActionResult> YourStatement(string organisationIdentifier, int year)
        {
            var getViewModelResult = SubmissionPresenter.GetYourStatementPageViewModel(organisationIdentifier, year);
            if (!getViewModelResult.Outcome==Success)return getViewModelResult.

            return View(new StatementViewModel { OrganisationIdentifier = organisationIdentifier, Year = year });
            //var checkResult = await CheckUserRegisteredOkAsync();
            //if (checkResult != null) return checkResult;

            //var result = await SubmissionPresenter.TryGetYourStatement(CurrentUser, organisationIdentifier, year);

            //return await GetActionResultFromQuery(result);
        }

        [HttpPost("{organisationIdentifier}/{year}/your-statement")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YourStatement(StatementViewModel submissionModel)
        {

            // Redirect location
            var next = await SubmissionPresenter.GetNextRedirectAction(SubmissionStep.YourStatement);
            return RedirectToAction(next, new { organisationIdentifier = submissionModel.OrganisationIdentifier, year = submissionModel.Year });
            //var result = await SubmissionPresenter.TrySaveYourStatement(CurrentUser, submissionModel);

            //return await GetActionResultFromSave(submissionModel, result, SubmissionStep.YourStatement);
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
        public async Task<IActionResult> Compliance(StatementViewModel submissionModel)
        {
            var result = await SubmissionPresenter.TrySaveCompliance(CurrentUser, submissionModel);

            return await GetActionResultFromSave(submissionModel, result, SubmissionStep.Compliance);
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

        [HttpGet("{organisationIdentifier}/{year}/your-organisation")]
        public async Task<IActionResult> YourOrganisation(string organisationIdentifier, int year)
        {
            return View(new StatementViewModel
            {
                OrganisationIdentifier = organisationIdentifier,
                Year = year,
            });

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var result = await SubmissionPresenter.TryGetYourOrganisation(CurrentUser, organisationIdentifier, year);

            return await GetActionResultFromQuery(result);
        }

        [HttpPost("{organisationIdentifier}/{year}/your-organisation")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YourOrganisation(StatementViewModel submissionModel)
        {
            // Redirect location
            var next = await SubmissionPresenter.GetNextRedirectAction(SubmissionStep.YourOrganisation);
            return RedirectToAction(next, new { organisationIdentifier = submissionModel.OrganisationIdentifier, year = submissionModel.Year });

        }

        [HttpPost("cancel-your-organisation")]
        public async Task<IActionResult> CancelYourOrganisation()
        {
            await SubmissionPresenter.ClearDraftForUser();

            var next = await SubmissionPresenter.GetCancelRedirection();
            return RedirectToAction(next);
        }

        #endregion

        #region Step 4 - Policies

        [HttpGet("{organisationIdentifier}/{year}/policies")]
        public async Task<IActionResult> Policies(string organisationIdentifier, int year)
        {
            return View(new StatementViewModel { OrganisationIdentifier = organisationIdentifier, Year = year });

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var result = await SubmissionPresenter.TryGetPolicies(CurrentUser, organisationIdentifier, year);

            return await GetActionResultFromQuery(result);
        }

        [HttpPost("{organisationIdentifier}/{year}/policies")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Policies(StatementViewModel submissionModel)
        {
            // Redirect location
            var next = await SubmissionPresenter.GetNextRedirectAction(SubmissionStep.Policies);
            return RedirectToAction(next, new { organisationIdentifier = submissionModel.OrganisationIdentifier, year = submissionModel.Year });

        }

        [HttpPost("cancel-policies")]
        public async Task<IActionResult> CancelPolicies()
        {
            await SubmissionPresenter.ClearDraftForUser();

            var next = await SubmissionPresenter.GetCancelRedirection();
            return RedirectToAction(next);
        }

        #endregion

        #region Step 5 - Supply chain risks and due diligence

        [HttpGet("{organisationIdentifier}/{year}/supply-chain-risks")]
        public async Task<IActionResult> SupplyChainRisks(string organisationIdentifier, int year)
        {
            return View(new StatementViewModel
            {
                OrganisationIdentifier = organisationIdentifier,
                Year = year,
                RelevantRiskTypes = new List<StatementRiskType>()
                //StatementRiskTypes = SharedBusinessLogic.DataRepository.GetAll<StatementRiskType>().Where(x => x.ParentRiskTypeId != x.StatementRiskTypeId).ToList(),

            });

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var result = await SubmissionPresenter.TryGetSupplyChainRiskAndDueDiligence(CurrentUser, organisationIdentifier, year);

            return await GetActionResultFromQuery(result);
        }

        [HttpPost("{organisationIdentifier}/{year}/supply-chain-risks")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupplyChainRisks(StatementViewModel submissionModel)
        {
            // Redirect location
            var next = await SubmissionPresenter.GetNextRedirectAction(SubmissionStep.SupplyChainRisks);
            return RedirectToAction(next, new { organisationIdentifier = submissionModel.OrganisationIdentifier, year = submissionModel.Year });

        }

        [HttpPost("cancel-supply-chain-risks")]
        public async Task<IActionResult> CancelSupplyChainRisks()
        {
            await SubmissionPresenter.ClearDraftForUser();

            var next = await SubmissionPresenter.GetCancelRedirection();
            return RedirectToAction(next);
        }

        #endregion

        #region Step 6 - Due Diligence

        [HttpGet("{organisationIdentifier}/{year}/due-diligence")]
        public async Task<IActionResult> DueDiligence(string organisationIdentifier, int year)
        {
            return View(new StatementViewModel
            {
                OrganisationIdentifier = organisationIdentifier,
                Year = year,
                DiligenceTypes = new List<StatementDiligenceType>()
                // DiligenceTypes = SharedBusinessLogic.DataRepository.GetAll<StatementDiligenceType>().Where(x => x.ParentDiligenceTypeId != x.StatementDiligenceTypeId).ToList(),


            });

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var result = await SubmissionPresenter.TryGetSupplyChainRiskAndDueDiligence(CurrentUser, organisationIdentifier, year);

            return await GetActionResultFromQuery(result);
        }

        [HttpPost("{organisationIdentifier}/{year}/due-diligence")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DueDiligence(StatementViewModel submissionModel)
        {
            // Redirect location
            var next = await SubmissionPresenter.GetNextRedirectAction(SubmissionStep.DueDiligence);
            return RedirectToAction(next, new { organisationIdentifier = submissionModel.OrganisationIdentifier, year = submissionModel.Year });

        }

        [HttpPost("cancel-due-diligence")]
        public async Task<IActionResult> CancelDueDiligence()
        {
            await SubmissionPresenter.ClearDraftForUser();

            var next = await SubmissionPresenter.GetCancelRedirection();
            return RedirectToAction(next);
        }

        #endregion

        #region Step 7 - Training

        [HttpGet("{organisationIdentifier}/{year}/training")]
        public async Task<IActionResult> Training(string organisationIdentifier, int year)
        {
            return View(new StatementViewModel { OrganisationIdentifier = organisationIdentifier, Year = year });

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var result = await SubmissionPresenter.TryGetTraining(CurrentUser, organisationIdentifier, year);

            return await GetActionResultFromQuery(result);
        }

        [HttpPost("{organisationIdentifier}/{year}/training")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Training(StatementViewModel submissionModel)
        {
            // Redirect location
            var next = await SubmissionPresenter.GetNextRedirectAction(SubmissionStep.Training);
            return RedirectToAction(next, new { organisationIdentifier = submissionModel.OrganisationIdentifier, year = submissionModel.Year });

        }

        [HttpPost("cancel-training")]
        public async Task<IActionResult> CancelTraining()
        {
            await SubmissionPresenter.ClearDraftForUser();

            var next = await SubmissionPresenter.GetCancelRedirection();
            return RedirectToAction(next);
        }

        #endregion


        #region Step 8 - Monitoring progress

        [HttpGet("{organisationIdentifier}/{year}/monitoring-progress")]
        public async Task<IActionResult> MonitoringProgress(string organisationIdentifier, int year)
        {
            return View(new StatementViewModel { OrganisationIdentifier = organisationIdentifier, Year = year });

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var result = await SubmissionPresenter.TryGetMonitoringInProgress(CurrentUser, organisationIdentifier, year);

            return await GetActionResultFromQuery(result);
        }

        [HttpPost("{organisationIdentifier}/{year}/monitoring-progress")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MonitoringProgress(StatementViewModel submissionModel)
        {
            // Redirect location
            var next = await SubmissionPresenter.GetNextRedirectAction(SubmissionStep.MonitoringProgress);
            return RedirectToAction(next, new { organisationIdentifier = submissionModel.OrganisationIdentifier, year = submissionModel.Year });

        }

        [HttpPost("cancel-monitoring-progress")]
        public async Task<IActionResult> CancelMonitoringProgress()
        {
            await SubmissionPresenter.ClearDraftForUser();

            var next = await SubmissionPresenter.GetCancelRedirection();
            return RedirectToAction(next);
        }

        #endregion

        #region Step 9 - Review

        [HttpGet("{organisationIdentifier}/{year}/review")]
        public async Task<IActionResult> ReviewAndEdit(string identifier, string organisationIdentifier, int year) // an ID parameter
        {

            return View(new StatementViewModel { OrganisationIdentifier = organisationIdentifier, Year = year });

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

        private async Task<IActionResult> GetActionResultFromQuery(CustomResult<StatementViewModel> result)
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

        private async Task<IActionResult> GetActionResultFromSave(StatementViewModel viewModel, StatementActionResult result, SubmissionStep step)
        {
            if (result != StatementActionResult.Success)
            {
                ModelState.AddModelError("Failed", "Failed");
                return View(viewModel);
            }

            // Redirect location
            var next = await SubmissionPresenter.GetNextRedirectAction(step);
            return RedirectToAction(next, new { organisationIdentifier = viewModel.OrganisationIdentifier, year = viewModel.Year });
        }

        #endregion
    }
}
