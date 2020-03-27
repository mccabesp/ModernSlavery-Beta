using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;

namespace ModernSlavery.WebUI.Submission.Controllers
{
    public partial class SubmissionController : BaseController
    {
        #region private methods

        private static bool IsOrganisationSizeModified(ReturnViewModel postedReturnViewModel,
            ReturnViewModel stashedReturnViewModel)
        {
            return postedReturnViewModel.OrganisationSize != stashedReturnViewModel.OrganisationSize;
        }

        #endregion

        #region public methods

        [HttpGet("organisation-size")]
        public async Task<IActionResult> OrganisationSize(string returnUrl = null)
        {
            #region Check user, then retrieve model from Session

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var stashedReturnViewModel = UnstashModel<ReturnViewModel>();

            #endregion

            if (stashedReturnViewModel == null) return SessionExpiredView();

            stashedReturnViewModel =
                await LoadReturnViewModelFromDBorFromDraftFileAsync(stashedReturnViewModel, VirtualUser.UserId);

            if (!stashedReturnViewModel.ReportInfo.Draft.IsUserAllowedAccess)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CustomError", WebService.ErrorViewModelFactory.Create(3040));
            }

            stashedReturnViewModel.ReturnUrl = returnUrl;

            return View("OrganisationSize", stashedReturnViewModel);
        }

        [HttpPost("organisation-size")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrganisationSize(ReturnViewModel postedReturnViewModel,
            string returnUrl = null)
        {
            #region Check user, then retrieve model from Session

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var stashedReturnViewModel = UnstashModel<ReturnViewModel>();

            #endregion

            if (stashedReturnViewModel == null) return SessionExpiredView();

            postedReturnViewModel.ReportInfo = stashedReturnViewModel.ReportInfo;

            ModelState.Include(nameof(postedReturnViewModel.OrganisationSize));

            #region Keep draft file locked to this user

            await _SubmissionPresenter.KeepDraftFileLockedToUserAsync(postedReturnViewModel, CurrentUser.UserId);

            if (!postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession)
                postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession =
                    IsOrganisationSizeModified(postedReturnViewModel, stashedReturnViewModel);

            if (!stashedReturnViewModel.ReportInfo.Draft.IsUserAllowedAccess)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CustomError", WebService.ErrorViewModelFactory.Create(3040));
            }

            #endregion

            StashModel(postedReturnViewModel);

            return RedirectToAction(returnUrl.EqualsI("CheckData") ? "CheckData" : "EmployerWebsite");
        }

        [HttpPost("cancel-organisation-size")]
        public async Task<IActionResult> CancelOrganisationSize(ReturnViewModel postedReturnViewModel)
        {
            postedReturnViewModel.OriginatingAction = "OrganisationSize";
            return await ManageDraftAsync(postedReturnViewModel, IsOrganisationSizeModified);
        }

        #endregion
    }
}