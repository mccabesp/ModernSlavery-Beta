using Microsoft.AspNetCore.Mvc;
using ModernSlavery.WebUI.Shared.Models.HttpResultModels;

namespace ModernSlavery.WebUI.Viewing.Controllers
{

    public partial class ActionHubController
    {

        // GET: ActionHub
        [NonAction]
        public IActionResult Overview1()
        {
            if (!UseNewActionHub())
            {
                return View("/Views/ActionHub1/Overview.cshtml");
            }

            return new HttpNotFoundResult();
        }

        [HttpGet("effective-actions")]
        public IActionResult Effective()
        {
            if (!UseNewActionHub())
            {
                return View("/Views/ActionHub1/Effective.cshtml");
            }

            return new HttpNotFoundResult();
        }

        [HttpGet("promising-actions")]
        public IActionResult Promising()
        {
            if (!UseNewActionHub())
            {
                return View("/Views/ActionHub1/Promising.cshtml");
            }

            return new HttpNotFoundResult();
        }

        [HttpGet("actions-with-mixed-results")]
        public IActionResult MixedResult()
        {
            if (!UseNewActionHub())
            {
                return View("/Views/ActionHub1/MixedResult.cshtml");
            }

            return new HttpNotFoundResult();
        }

    }
}
