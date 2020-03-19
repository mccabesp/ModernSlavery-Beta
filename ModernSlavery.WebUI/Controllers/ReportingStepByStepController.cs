using Microsoft.AspNetCore.Mvc;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Controllers.ReportingStepByStep
{
    public class ReportingStepByStepController : Controller
    {

        [HttpGet("reporting-step-by-step")]
        [FeatureSwitch("ReportingStepByStep")]
        public IActionResult StepByStepStandalone()
        {
            return View("../ReportingStepByStep/StandalonePage");
        }

        [HttpGet("reporting-step-by-step/find-out-what-the-gender-pay-gap-is")]
        [FeatureSwitch("ReportingStepByStep")]
        public IActionResult Step1Task1()
        {
            return View("../ReportingStepByStep/Step1FindOutWhatTheGpgIs");
        }
        
        [HttpGet("reporting-step-by-step/report")]
        [FeatureSwitch("ReportingStepByStep")]
        public IActionResult Step6Task1()
        {
            return View("../ReportingStepByStep/Step6Task1");
        }
    }
}
