using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Options;

namespace ModernSlavery.WebUI.Submission.Controllers
{
    public partial class SubmissionController : BaseController
    {
        [HttpGet("submission-complete")]
        public async Task<IActionResult> SubmissionComplete()
        {
            #region Check user, then retrieve model from Session

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var stashedReturnViewModel = UnstashModel<ReturnViewModel>();

            #endregion

            if (stashedReturnViewModel == null)
                stashedReturnViewModel = await _SubmissionPresenter.GetReturnViewModelAsync(
                    ReportingOrganisationId,
                    ReportingOrganisationStartYear.Value,
                    VirtualUser.UserId);

            EmployerBackUrl = RequestUrl.PathAndQuery;
            ReportBackUrl = null;

            ClearStash();

            return View(stashedReturnViewModel);
        }

        [HttpPost("submission-complete")]
        [PreventDuplicatePost]
        public async Task<IActionResult> SubmissionCompletePost(string command)
        {
            var doneUrl = SharedBusinessLogic.SharedOptions.DoneUrl ??
                          await WebService.RouteHelper.Get(UrlRouteOptions.Routes.ViewingHome);

            return await LogoutUser(doneUrl);
        }
    }
}