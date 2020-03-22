using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessLogic.Models.Submit;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Controllers;

namespace ModernSlavery.WebUI.Controllers
{
    public partial class SubmitController : BaseController
    {

        [HttpGet("submission-complete")]
        public async Task<IActionResult> SubmissionComplete()
        {
            #region Check user, then retrieve model from Session

            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            var stashedReturnViewModel = this.UnstashModel<ReturnViewModel>();

            #endregion

            if (stashedReturnViewModel == null)
            {
                stashedReturnViewModel = await _SubmissionPresenter.GetReturnViewModelAsync(
                    ReportingOrganisationId,
                    ReportingOrganisationStartYear.Value,
                    currentUser.UserId);
            }

            EmployerBackUrl = RequestUrl.PathAndQuery;
            ReportBackUrl = null;

            this.ClearStash();

            return View(stashedReturnViewModel);
        }

        [HttpPost("submission-complete")]
        [PreventDuplicatePost]
        public IActionResult SubmissionCompletePost(string command)
        {
            string doneUrl = CommonBusinessLogic.GlobalOptions.DoneUrl ?? Url.Action("Index", "Viewing", null, "https");

            return LogoutUser(doneUrl);
        }

    }
}
