using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;

namespace ModernSlavery.WebUI.Submission.Controllers
{
    public partial class SubmissionController
    {
        [Authorize]
        [HttpGet("~/report-for-organisation/{request}")]
        public async Task<IActionResult> ReportForOrganisation(string request)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            // Decrypt request
            if (!request.DecryptToParams(out var requestParams))
                return new HttpBadRequestResult($"Cannot decrypt parameters '{request}'");

            // Extract the request vars
            var organisationId = requestParams[0].ToInt64();
            var reportingStartYear = requestParams[1].ToInt32();
            var change = requestParams[2].ToBoolean();

            // Ensure we can report for the year requested
            if (!_SubmissionPresenter.IsValidSnapshotYear(reportingStartYear))
                return new HttpBadRequestResult($"Invalid snapshot year {reportingStartYear}");

            // Check the user has permission for this organisation
            var userOrg = VirtualUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
                return new HttpForbiddenResult(
                    $"User {VirtualUser?.EmailAddress} is not registered for organisation id {organisationId}");

            // get the sector
            var sectorType = userOrg.Organisation.SectorType;

            // Determine if this is for the previous reporting year
            var isPrevReportingYear =
                _SubmissionPresenter.IsCurrentSnapshotYear(sectorType, reportingStartYear) == false;

            // Set the reporting session globals
            ReportingOrganisationId = organisationId;
            ReportingOrganisationStartYear = reportingStartYear;

            // Clear the SubmitController stash
            ClearAllStashes();

            await _SubmissionPresenter.RestartDraftFileAsync(organisationId, reportingStartYear, VirtualUser.UserId);

            // When previous reporting year then do late submission flow
            if (isPrevReportingYear)
            {
                // Change an existing late submission
                if (change) return RedirectToAction("LateWarning", new {request, returnUrl = "CheckData"});

                // Create new a late submission
                return RedirectToAction("LateWarning", new {request});
            }

            /*
 * Under normal circumstances, we might want to stash the model at this point, just before the redirection, however, we are NOT going to for two reasons:
 *      (1) The information currently on the model includes ONLY the bare minimum to know if there is a draft or not, it doesn't for example, include anything to do with the permissions to access, who is locked it, lastWrittenTimestamp... This behaviour is by design: the draft file is locked on access, and that will happen once the user arrives to 'check data' or 'enter calculations', if we were to stash the model now, the stashed info won't contain all relevant draft information.
 *      (2) Currently stash/unstash only works with the name of the controller, so it really doesn't matter what we stash here, the 'check data' and 'enter calculations' page belong to a different controller, so the stashed info will never be read by them anyway.
 */
            // Change an existing submission
            if (change) return RedirectToAction("CheckData");

            // Create new a submission
            return RedirectToAction("EnterCalculations");
        }
    }
}