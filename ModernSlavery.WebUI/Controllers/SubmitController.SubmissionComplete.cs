using System.Threading.Tasks;
using ModernSlavery.BusinessLogic.Models.Submit;
using ModernSlavery.Core;
using ModernSlavery.Database;
using ModernSlavery.WebUI.Classes;
using Microsoft.AspNetCore.Mvc;

namespace ModernSlavery.WebUI.Controllers.Submission
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
                stashedReturnViewModel = await submissionService.GetReturnViewModelAsync(
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
            string doneUrl = Global.DoneUrl ?? Url.Action("Index", "Viewing", null, "https");

            return LogoutUser(doneUrl);
        }

    }
}
