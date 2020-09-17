using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Shared.Controllers
{
    [Route("Error")]
    public class ErrorController : BaseController
    {
        #region Constructors

        public ErrorController(
            ILogger<ErrorController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(
            logger, webService, sharedBusinessLogic)
        {
        }

        #endregion

        [HttpGet("/error/")]
        [HttpGet("/error/{errorCode?}")]
        public IActionResult Default(int errorCode = 500)
        {
            if (errorCode == 0)
            {
                if (Response.StatusCode.Between(400, 599))
                    errorCode = Response.StatusCode;
                else
                    errorCode = 500;
            }

            var model = WebService.ErrorViewModelFactory.Create(errorCode);

            //Get the exception which caused this error
            var errorData = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (errorData == null)
            {
                //Log non-exception events
                var statusCodeData = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
                if (statusCodeData != null)

                {
                    if (errorCode == 404 || errorCode == 405)
                        Logger.LogWarning($"HttpStatusCode {errorCode}, Path: {statusCodeData.OriginalPath}");
                    else if (errorCode >= 400)
                        Logger.LogError($"HttpStatusCode {errorCode}, Path: {statusCodeData.OriginalPath}");
                }
            }

            Response.StatusCode = errorCode;
            return View("CustomError", model);
        }

        [HttpGet("/error/service-unavailable")]
        public IActionResult ServiceUnavailable()
        {
            var model = WebService.ErrorViewModelFactory.Create(1119);
            Response.StatusCode = 503;
            return View("CustomError", model);
        }
    }
}