using Microsoft.AspNetCore.Mvc;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;

namespace ModernSlavery.WebUI.Viewing.Controllers
{
    public partial class ActionHubController
    {
        // GET: ActionHub
        [NonAction]
        public IActionResult Overview1()
        {
            if (!UseNewActionHub()) return View("/Areas/Viewing/Views/ActionHub/Old/Overview.cshtml");

            return new HttpNotFoundResult();
        }

        [HttpGet("effective-actions")]
        public IActionResult Effective()
        {
            if (!UseNewActionHub()) return View("/Areas/Viewing/Views/ActionHub/Old/Effective.cshtml");

            return new HttpNotFoundResult();
        }

        [HttpGet("promising-actions")]
        public IActionResult Promising()
        {
            if (!UseNewActionHub()) return View("/Areas/Viewing/Views/ActionHub/Old/Promising.cshtml");

            return new HttpNotFoundResult();
        }

        [HttpGet("actions-with-mixed-results")]
        public IActionResult MixedResult()
        {
            if (!UseNewActionHub()) return View("/Areas/Viewing/Views/ActionHub/Old/MixedResult.cshtml");

            return new HttpNotFoundResult();
        }
    }
}