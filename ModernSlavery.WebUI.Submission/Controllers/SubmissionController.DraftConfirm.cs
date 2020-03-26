﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.BusinessDomain.Submission.Models;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;

namespace ModernSlavery.WebUI.Submission.Controllers
{
    public partial class SubmissionController
    {

        #region private methods

        [HttpPost("exit-without-saving")]
        public async Task<IActionResult> ExitWithoutSaving()
        {
            #region Check user, then retrieve model from Session

            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            var stashedReturnViewModel = this.UnstashModel<ReturnViewModel>();

            #endregion

            if (stashedReturnViewModel.HasReported())
            {
                await _SubmissionPresenter.DiscardDraftFileAsync(stashedReturnViewModel);
            }
            else
            {
                await _SubmissionPresenter.RollbackDraftFileAsync(stashedReturnViewModel);
            }

            return RedirectToAction(Url.Action(nameof(SubmissionController.ManageOrganisations)));
        }

        [HttpPost("save-draft")]
        public async Task<IActionResult> SaveDraftAsync(ReturnViewModel postedReturnViewModel)
        {
            #region Check user, then retrieve model from Session

            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            var stashedReturnViewModel = this.UnstashModel<ReturnViewModel>();

            #endregion

            postedReturnViewModel.ReportInfo = stashedReturnViewModel.ReportInfo;

            ExcludeBlankFieldsFromModelState(postedReturnViewModel);

            ConfirmPayBandsAddUpToOneHundred(postedReturnViewModel);

            ValidateBonusIntegrity(postedReturnViewModel);

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View(postedReturnViewModel.OriginatingAction, postedReturnViewModel);
            }

            await _SubmissionPresenter.UpdateDraftFileAsync(currentUser.UserId, postedReturnViewModel);
            await _SubmissionPresenter.CommitDraftFileAsync(postedReturnViewModel);

            return View("DraftComplete", postedReturnViewModel);
        }

        #endregion

    }
}