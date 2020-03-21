using System.Threading.Tasks;
using ModernSlavery.BusinessLogic.Models.Submit;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.Entities;

namespace ModernSlavery.WebUI.Controllers.Submission
{
    public partial class SubmitController
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

            return RedirectToAction(nameof(OrganisationController.ManageOrganisations), "Organisation");
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
