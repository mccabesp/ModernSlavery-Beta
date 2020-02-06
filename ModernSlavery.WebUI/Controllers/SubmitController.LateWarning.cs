using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.BusinessLogic.Models.Submit;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Models.HttpResultModels;
using ModernSlavery.Database;
using ModernSlavery.Extensions;
using ModernSlavery.WebUI.Classes;
using Microsoft.AspNetCore.Mvc;

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

            if (!submissionService.IsValidSnapshotYear(reportingYear))
            {
                return new HttpBadRequestResult($"Invalid snapshot year {reportingYear}");
            }

            stashedReturnViewModel = await LoadReturnViewModelFromDBorFromDraftFileAsync(stashedReturnViewModel, currentUser.UserId);

            if (stashedReturnViewModel?.ReportInfo != null
                && !stashedReturnViewModel.ReportInfo.Draft.IsUserAllowedAccess)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CustomError", new ErrorViewModel(3040));
            }

            stashedReturnViewModel.ReturnUrl = returnUrl ?? "EnterCalculations";
            stashedReturnViewModel.ShouldProvideLateReason = true;

            this.StashModel(stashedReturnViewModel);

            return View(stashedReturnViewModel);
        }

    }
}
