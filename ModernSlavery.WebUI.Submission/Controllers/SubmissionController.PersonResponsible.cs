using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;

namespace ModernSlavery.WebUI.Submission.Controllers
{
    public partial class SubmissionController : BaseController
    {

        #region private methods

        private static bool IsPersonResponsibleModified(ReturnViewModel postedReturnViewModel, ReturnViewModel stashedReturnViewModel)
        {
            bool hasFirstNameChanged =
                stashedReturnViewModel != null && postedReturnViewModel.FirstName != stashedReturnViewModel.FirstName;
            bool hasLastNameChanged = stashedReturnViewModel != null && postedReturnViewModel.LastName != stashedReturnViewModel.LastName;
            bool hasJobTitleChanged = stashedReturnViewModel != null && postedReturnViewModel.JobTitle != stashedReturnViewModel.JobTitle;
            return hasFirstNameChanged || hasLastNameChanged || hasJobTitleChanged;
        }

        #endregion

        #region public methods

        [HttpGet("person-responsible")]
        public async Task<IActionResult> PersonResponsible(string returnUrl = null)
        {
            #region Check user, then retrieve model from Session

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null)
            {
                return checkResult;
            }

            var stashedReturnViewModel = this.UnstashModel<ReturnViewModel>();

            #endregion

            if (stashedReturnViewModel == null)
            {
                return SessionExpiredView();
            }

            stashedReturnViewModel = await LoadReturnViewModelFromDBorFromDraftFileAsync(stashedReturnViewModel, VirtualUser.UserId);

            if (!stashedReturnViewModel.ReportInfo.Draft.IsUserAllowedAccess)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CustomError", WebService.ErrorViewModelFactory.Create(3040));
            }

            stashedReturnViewModel.ReturnUrl = returnUrl;

            return View("PersonResponsible", stashedReturnViewModel);
        }

        [HttpPost("person-responsible")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PersonResponsible(ReturnViewModel postedReturnViewModel, string returnUrl = null)
        {
            #region Check user, then retrieve model from Session

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null)
            {
                return checkResult;
            }

            var stashedReturnViewModel = this.UnstashModel<ReturnViewModel>();

            #endregion

            if (stashedReturnViewModel == null)
            {
                return SessionExpiredView();
            }

            postedReturnViewModel.ReportInfo = stashedReturnViewModel.ReportInfo;

            ExcludeBlankFieldsFromModelState(postedReturnViewModel);

            ModelState.Include(
                nameof(postedReturnViewModel.FirstName),
                nameof(postedReturnViewModel.LastName),
                nameof(postedReturnViewModel.JobTitle));

            #region Keep draft file locked to this user

            await _SubmissionPresenter.KeepDraftFileLockedToUserAsync(postedReturnViewModel, CurrentUser.UserId);

            if (!postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession)
            {
                postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession =
                    IsPersonResponsibleModified(postedReturnViewModel, stashedReturnViewModel);
            }

            if (!stashedReturnViewModel.ReportInfo.Draft.IsUserAllowedAccess)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CustomError", WebService.ErrorViewModelFactory.Create(3040));
            }

            #endregion

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("PersonResponsible", postedReturnViewModel);
            }

            this.StashModel(postedReturnViewModel);

            return RedirectToAction(returnUrl.EqualsI("CheckData") ? "CheckData" : "OrganisationSize");
        }

        [HttpPost("cancel-person-responsible")]
        public async Task<IActionResult> CancelPersonResponsible(ReturnViewModel postedReturnViewModel)
        {
            postedReturnViewModel.OriginatingAction = "PersonResponsible";
            return await ManageDraftAsync(postedReturnViewModel, IsPersonResponsibleModified);
        }

        #endregion

    }
}
