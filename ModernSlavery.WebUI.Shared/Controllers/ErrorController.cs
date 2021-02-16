using System.Net;
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

        [HttpPost("/error/")]
        [HttpPost("/error/{errorCode?}")]
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
                if (statusCodeData != null && errorCode.Between(400, 599))
                {
                    if (errorCode >= 500)
                        Logger.LogError($"HttpStatusCode {errorCode}, Path: {statusCodeData.OriginalPath}");
                    else
                        Logger.LogWarning($"HttpStatusCode {errorCode}, Path: {statusCodeData.OriginalPath}");
                }
            }

            return View("CustomError", model);
        }

        [HttpPost("/error/service-unavailable")]
        [HttpGet("/error/service-unavailable")]
        public IActionResult ServiceUnavailable()
        {
            var model = WebService.ErrorViewModelFactory.Create(503);
            return View("CustomError", model);
        }

        [Route("/{*url}", Order = int.MaxValue)]
        public IActionResult CatchAll()
        {
            var errorCode = (int)HttpStatusCode.NotFound;
            var model = WebService.ErrorViewModelFactory.Create(errorCode);
            Logger.LogWarning($"HttpStatusCode {404}, Path: {HttpContext.GetUri().PathAndQuery}");
            return View("CustomError", model);
        }
    }
}