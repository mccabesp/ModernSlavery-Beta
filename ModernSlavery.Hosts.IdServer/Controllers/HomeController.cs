// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessLogic;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.IdServer.Controllers
{
    [Route("Home")]
    public class HomeController : BaseController
    {

        private readonly IEventService _events;
        private readonly IIdentityServerInteractionService _interaction;

        public HomeController(
            IIdentityServerInteractionService interaction,
            IEventService events,
            ILogger<HomeController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            _interaction = interaction;
            _events = events;
        }

        [Route("~/ping")]
        public IActionResult Ping()
        {
            return new OkResult(); // OK = 200
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("~/login/login")]
        public IActionResult RedirectOldLogin()
        {
            return Redirect(SharedBusinessLogic.SharedOptions.SiteAuthority);
        }

        /// <summary>
        ///     Shows the error page
        /// </summary>
        [Route("~/error/{errorId?}")]
        public async Task<IActionResult> Error(string errorId = null)
        {
            int errorCode = errorId.ToInt32();

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


            // retrieve error details from identityserver
            ErrorMessage message = await _interaction.GetErrorContextAsync(errorId);
            if (message != null)
            {
                Logger.LogError($"{message.Error}: {message.ErrorDescription}");
            }
            else
            {
                //Get the exception which caused this error
                var errorData = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
                if (errorData == null)
                {
                    //Log non-exception events
                    var statusCodeData = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
                    if (statusCodeData != null)
                    {
                        Logger.LogError($"HttpStatusCode {errorCode}, Path: {statusCodeData.OriginalPath}");
                    }
                    else
                    {
                        Logger.LogError($"HttpStatusCode {errorCode}, Path: Unknown");
                    }
                }
            }

            Response.StatusCode = errorCode;
            var model = WebService.ErrorViewModelFactory.Create(message.Error, message.ErrorDescription);
            return View("Error", model);
        }

    }
}
