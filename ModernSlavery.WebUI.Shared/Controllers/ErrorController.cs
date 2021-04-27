using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
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
        public async Task<IActionResult> Default(int errorCode = 500,[RelativeUrl] string redirectUrl=null)
        {
            //Must catch any errors here otherwise may get in a loop
            try
            {
                if (errorCode == 0)
                {
                    if (Response.StatusCode.Between(400, 599))
                        errorCode = Response.StatusCode;
                    else
                        errorCode = 500;
                }

                //Get the exception which caused this error
                var errorData = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
                if (errorData?.Error != null)
                {
                    Logger.LogError(errorData.Error, $"ErrorCode {errorCode}, Path: {errorData.Path}, Referrer: {UrlReferrer}");
                }
                else
                {
                    //Log non-exception events
                    var statusCodeData = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
                    if (statusCodeData != null && errorCode.Between(400, 599))
                    {
                        if (errorCode >= 500)
                            Logger.LogError(new Exception(), $"HttpStatusCode {errorCode}, Path: {statusCodeData.OriginalPath}, Referrer: {UrlReferrer}");
                        else
                            Logger.LogWarning($"HttpStatusCode {errorCode}, Path: {statusCodeData.OriginalPath}, Referrer: {UrlReferrer}");
                    }
                }

                var model = WebService.ErrorViewModelFactory.Create(errorCode);

                //Use the record url from the querystring
                if (!string.IsNullOrWhiteSpace(redirectUrl))model.ActionUrl = redirectUrl;

                return View("CustomError", model);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"ErrorCode {errorCode}, Path: {Request.Path}, Referrer: {UrlReferrer}");
                Response.ContentType = "text/plain";
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await Response.WriteAsync($"ERROR {Response.StatusCode}: Internal Server Error");
                return new EmptyResult();
            }
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