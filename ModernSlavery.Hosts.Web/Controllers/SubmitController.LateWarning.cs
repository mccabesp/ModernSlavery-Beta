using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.BusinessLogic.Models.Submit;
using ModernSlavery.Extensions;
using ModernSlavery.WebUI.Classes;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.Entities;
using ModernSlavery.WebUI.Shared.Models.HttpResultModels;

namespace ModernSlavery.WebUI.Controllers.Submission
{
    public partial class SubmitController : BaseController
    {

        [HttpGet("late-warning")]
        public async Task<IActionResult> LateWarning(string request, string returnUrl = null)
        {
            #region Check user, then retrieve model from session

            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            var stashedReturnViewModel = this.UnstashModel<ReturnViewModel>();

            #endregion

            if (!request.DecryptToParams(out List<string> requestParams))
            {
                return new HttpBadRequestResult($"Cannot descrypt request parameters '{request}'");
            }

            int organisationId = requestParams[0].ToInt32();
            int reportingYear = requestParams[1].ToInt32();

            if (!_SubmissionPresenter.IsValidSnapshotYear(reportingYear))
            {
                return new HttpBadRequestResult($"Invalid snapshot year {reportingYear}");
            }

            stashedReturnViewModel = await LoadReturnViewModelFromDBorFromDraftFileAsync(stashedReturnViewModel, currentUser.UserId);

            if (stashedReturnViewModel?.ReportInfo != null
                && !stashedReturnViewModel.ReportInfo.Draft.IsUserAllowedAccess)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CustomError", WebService.ErrorViewModelFactory.Create(3040));
            }

            stashedReturnViewModel.ReturnUrl = returnUrl ?? "EnterCalculations";
            stashedReturnViewModel.ShouldProvideLateReason = true;

            this.StashModel(stashedReturnViewModel);

            return View(stashedReturnViewModel);
        }

    }
}
