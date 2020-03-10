﻿using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Abstractions;

namespace ModernSlavery.WebUI.Controllers
{
    [Route("Error")]
    public class ErrorController : BaseController
    {

        #region Constructors

        public ErrorController(
            ILogger<ErrorController> logger,
            IWebService webService,
            IDataRepository dataRepository, IFileRepository fileRepository) : base(logger, webService, dataRepository, fileRepository)
        { }

        #endregion

        [Route("/error/")]
        [Route("/error/{errorCode?}")]
        public IActionResult Default(int errorCode = 500)
        {
            if (errorCode == 0)
            {
                if (Response.StatusCode.Between(400, 599))
                {
                    errorCode = Response.StatusCode;
                }
                else
                {
                    errorCode = 500;
                }
            }

            var model = new ErrorViewModel(errorCode);

            //Get the exception which caused this error
            var errorData = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (errorData == null)
            {
                //Log non-exception events
                var statusCodeData = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
                if (statusCodeData != null)

                {
                    if (errorCode == 404 || errorCode == 405)
                    {
                        Logger.LogWarning($"HttpStatusCode {errorCode}, Path: {statusCodeData.OriginalPath}");
                    }
                    else if (errorCode >= 400)
                    {
                        Logger.LogError($"HttpStatusCode {errorCode}, Path: {statusCodeData.OriginalPath}");
                    }
                }
            }

            Response.StatusCode = errorCode;
            return View("CustomError", model);
        }

        [Route("/error/service-unavailable")]
        public IActionResult ServiceUnavailable()
        {
            var model = new ErrorViewModel(1119);
            Response.StatusCode = 503;
            return View("CustomError", model);
        }

    }
}
