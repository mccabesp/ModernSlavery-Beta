using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Controllers;

namespace ModernSlavery.WebUI.Submission.Controllers
{
    public partial class SubmissionController : BaseController
    {
        [HttpGet("late-warning")]
        public async Task<IActionResult> LateWarning(string request, string returnUrl = null)
        {
            #region Check user, then retrieve model from session

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var stashedReturnViewModel = UnstashModel<ReturnViewModel>();

            #endregion

            if (!request.DecryptToParams(out var requestParams))
                return new HttpBadRequestResult($"Cannot descrypt request parameters '{request}'");

            var organisationId = requestParams[0].ToInt32();
            var reportingYear = requestParams[1].ToInt32();

            if (!_SubmissionPresenter.IsValidSnapshotYear(reportingYear))
                return new HttpBadRequestResult($"Invalid snapshot year {reportingYear}");

            stashedReturnViewModel =
                await LoadReturnViewModelFromDBorFromDraftFileAsync(stashedReturnViewModel, VirtualUser.UserId);

            if (stashedReturnViewModel?.ReportInfo != null
                && !stashedReturnViewModel.ReportInfo.Draft.IsUserAllowedAccess)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CustomError", WebService.ErrorViewModelFactory.Create(3040));
            }

            stashedReturnViewModel.ReturnUrl = returnUrl ?? "EnterCalculations";
            stashedReturnViewModel.ShouldProvideLateReason = true;

            StashModel(stashedReturnViewModel);

            return View(stashedReturnViewModel);
        }
    }
}