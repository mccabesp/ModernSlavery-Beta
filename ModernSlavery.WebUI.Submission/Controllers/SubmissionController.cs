using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.BusinessDomain.Submission.Models;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Models.HttpResultModels;
using ModernSlavery.WebUI.Submission.Classes;
using ModernSlavery.WebUI.Submission.Models;

namespace ModernSlavery.WebUI.Submission.Controllers
{
    [Authorize]
    [Route("Submit")]
    public partial class SubmissionController : BaseController
    {

        public delegate bool IsPageChanged(ReturnViewModel postedReturnViewModel, ReturnViewModel stashedReturnViewModel);

        private readonly ISubmissionPresenter _SubmissionPresenter;
        private readonly ISubmissionService _SubmissionService;

        public SubmissionController(
            ISubmissionService submissionService,
            ISubmissionPresenter submissionPresenter,
            ILogger<SubmissionController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            _SubmissionService = submissionService;
            _SubmissionPresenter = submissionPresenter;
        }

        [Route("Init")]
        public IActionResult Init()
        {
            if (!SharedBusinessLogic.SharedOptions.IsProduction())
            {
                Logger.LogInformation("Submit Controller Initialised");
            }

            return new EmptyResult();
        }

        [Authorize]
        [HttpGet("~/manage-organisations")]
        public IActionResult ManageOrganisations()
        {
            //Clear all the stashes
            this.ClearAllStashes();

            //Remove any previous searches from the cache
            this._SubmissionService.PrivateSectorRepository.ClearSearch();

            //Reset the current reporting organisation
            ReportingOrganisation = null;

            //Ensure user has completed the registration process
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null && IsImpersonatingUser == false)
            {
                return checkResult;
            }

            // check if the user has accepted the privacy statement (unless admin or impersonating)
            if (!IsImpersonatingUser && !base.CurrentUser.IsAdministrator())
            {
                DateTime? hasReadPrivacy = currentUser.AcceptedPrivacyStatement;
                if (hasReadPrivacy == null || hasReadPrivacy.Value < SharedBusinessLogic.SharedOptions.PrivacyChangedDate)
                {
                    return RedirectToAction(nameof(PrivacyPolicy), "Home");
                }
            }

            //create the new view model 
            IOrderedEnumerable<UserOrganisation> model = currentUser.UserOrganisations.OrderBy(uo => uo.Organisation.OrganisationName);
            return View(nameof(ManageOrganisations), model);
        }

        [Authorize]
        [HttpGet("~/manage-organisations/{id}")]
        public async Task<IActionResult> ManageOrganisation(string id)
        {
            //Ensure user has completed the registration process
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // Decrypt org id
            if (!id.DecryptToId(out long organisationId))
            {
                return new HttpBadRequestResult($"Cannot decrypt organisation id {id}");
            }

            // Check the user has permission for this organisation
            UserOrganisation userOrg = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
            {
                return new HttpForbiddenResult($"User {currentUser?.EmailAddress} is not registered for organisation id {organisationId}");
            }

            // clear the stash
            this.ClearStash();

            //Get the current snapshot date
            DateTime currentSnapshotDate = SharedBusinessLogic.GetAccountingStartDate(userOrg.Organisation.SectorType);

            //Make sure we have an explicit scope for last and year for organisations new to this year
            if (userOrg.PINConfirmedDate != null && userOrg.Organisation.Created >= currentSnapshotDate)
            {
                ScopeStatuses scopeStatus =
                    await _SubmissionService.ScopeBusinessLogic.GetLatestScopeStatusForSnapshotYearAsync(organisationId, currentSnapshotDate.Year - 1);
                if (!scopeStatus.IsAny(ScopeStatuses.InScope, ScopeStatuses.OutOfScope))
                {
                    return RedirectToAction(nameof(ScopeController.DeclareScope), "Scope", new { id });
                }
            }

            // get any associated users for the current org
            List<UserOrganisation> associatedUserOrgs = userOrg.GetAssociatedUsers().ToList();

            // get all editable reports
            List<ReportInfoModel> reportInfos = await _SubmissionPresenter.GetAllEditableReportsAsync(userOrg, currentSnapshotDate);

            // build the view model
            var model = new ManageOrganisationModel
            {
                CurrentUserOrg = userOrg,
                AssociatedUserOrgs = associatedUserOrgs,
                EncCurrentOrgId = Encryption.EncryptQuerystring(organisationId.ToString()),
                ReportInfoModels = reportInfos.OrderBy(r => r.ReportingStartDate).ToList()
            };

            return View(model);
        }

        [HttpGet("submit/")]
        public async Task<IActionResult> Redirect()
        {
            await TrackPageViewAsync();

            return RedirectToAction("EnterCalculations");
        }


        #region private methods

        private async Task<IActionResult> ManageDraftAsync(ReturnViewModel postedReturnViewModel, IsPageChanged isPageChangedLogic)
        {
            var stashedReturnViewModel = this.UnstashModel<ReturnViewModel>();
            if (stashedReturnViewModel == null)
            {
                return SessionExpiredView();
            }

            postedReturnViewModel.ReportInfo = stashedReturnViewModel.ReportInfo;

            if (!postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession)
            {
                postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession =
                    isPageChangedLogic(postedReturnViewModel, stashedReturnViewModel);
            }

            // As part of the cancellation flow, it's down to the user to confirm if the draft needs saving.
            return await PresentUserTheOptionOfSaveDraftOrIgnoreAsync(
                postedReturnViewModel,
                postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession);
        }

        private async Task<IActionResult> PresentUserTheOptionOfSaveDraftOrIgnoreAsync(ReturnViewModel postedReturnViewModel,
            bool hasADraftBeenCreated)
        {
            if (hasADraftBeenCreated)
            {
                this.StashModel(postedReturnViewModel);
                return View("DraftConfirm", postedReturnViewModel);
            }

            await _SubmissionPresenter.RollbackDraftFileAsync(postedReturnViewModel);
            return RedirectToAction(Url.Action(nameof(SubmissionController.ManageOrganisations)));
        }

        private void ExcludeBlankFieldsFromModelState(ReturnViewModel returnViewModel)
        {
            var setOfFieldsToExclude = new HashSet<string>();

            if (!returnViewModel.DiffMeanBonusPercent.HasValue || returnViewModel.DiffMeanBonusPercent.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.DiffMeanBonusPercent));
            }

            if (!returnViewModel.DiffMeanHourlyPayPercent.HasValue || returnViewModel.DiffMeanHourlyPayPercent.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.DiffMeanHourlyPayPercent));
            }

            if (!returnViewModel.DiffMedianBonusPercent.HasValue || returnViewModel.DiffMedianBonusPercent.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.DiffMedianBonusPercent));
            }

            if (!returnViewModel.DiffMedianHourlyPercent.HasValue || returnViewModel.DiffMedianHourlyPercent.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.DiffMedianHourlyPercent));
            }

            if (!returnViewModel.FemaleLowerPayBand.HasValue || returnViewModel.FemaleLowerPayBand.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.FemaleLowerPayBand));
            }

            if (!returnViewModel.FemaleMedianBonusPayPercent.HasValue || returnViewModel.FemaleMedianBonusPayPercent.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.FemaleMedianBonusPayPercent));
            }

            if (!returnViewModel.FemaleMiddlePayBand.HasValue || returnViewModel.FemaleMiddlePayBand.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.FemaleMiddlePayBand));
            }

            if (!returnViewModel.FemaleUpperPayBand.HasValue || returnViewModel.FemaleUpperPayBand.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.FemaleUpperPayBand));
            }

            if (!returnViewModel.FemaleUpperQuartilePayBand.HasValue || returnViewModel.FemaleUpperQuartilePayBand.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.FemaleUpperQuartilePayBand));
            }

            if (returnViewModel.MaleLowerPayBand == null)
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.MaleLowerPayBand));
            }

            if (!returnViewModel.MaleMedianBonusPayPercent.HasValue || returnViewModel.MaleMedianBonusPayPercent.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.MaleMedianBonusPayPercent));
            }

            if (!returnViewModel.MaleMiddlePayBand.HasValue || returnViewModel.MaleMiddlePayBand.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.MaleMiddlePayBand));
            }

            if (!returnViewModel.MaleUpperPayBand.HasValue || returnViewModel.MaleUpperPayBand.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.MaleUpperPayBand));
            }

            if (!returnViewModel.MaleUpperQuartilePayBand.HasValue || returnViewModel.MaleUpperQuartilePayBand.Value.Equals(null))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.MaleUpperQuartilePayBand));
            }

            if (string.IsNullOrWhiteSpace(returnViewModel.FirstName)
                && string.IsNullOrWhiteSpace(returnViewModel.LastName)
                && string.IsNullOrWhiteSpace(returnViewModel.JobTitle))
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.JobTitle));
                setOfFieldsToExclude.Add(nameof(returnViewModel.FirstName));
                setOfFieldsToExclude.Add(nameof(returnViewModel.LastName));
            }

            if (returnViewModel.LateReason.IsNullOrWhiteSpace())
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.LateReason));
            }

            if (returnViewModel.EHRCResponse.IsNullOrWhiteSpace())
            {
                setOfFieldsToExclude.Add(nameof(returnViewModel.EHRCResponse));
            }

            ModelState.Exclude(setOfFieldsToExclude.ToArray());
        }

        private async Task<ReturnViewModel> LoadReturnViewModelFromDBorFromDraftFileAsync(ReturnViewModel stashedReturnViewModel,
            long currentUserId)
        {
            if (stashedReturnViewModel == null)
            {
                /* info is not stashed, so load the return info from somewhere (db or draft-azure) */
                stashedReturnViewModel = await _SubmissionPresenter.GetReturnViewModelAsync(
                    ReportingOrganisationId,
                    ReportingOrganisationStartYear.Value,
                    currentUserId);
            }

            return stashedReturnViewModel;
        }

        #endregion

    }
}
