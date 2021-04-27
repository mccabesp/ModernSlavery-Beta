using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.WebUI.Shared.Models;

namespace ModernSlavery.WebUI.Shared.Classes.Middleware
{
    /// <summary>
    /// Handle AntiforgeryValidationException caused by sign-in or sign out from 2 open tabs
    /// </summary>
    public class RedirectAntiforgeryValidationFailedResultFilter : IAsyncAlwaysRunResultFilter
    {
        public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is AntiforgeryValidationFailedResult)
            {
                context.Result = new RedirectToActionResult("Default", "Error", new { errorCode = context.HttpContext.User.Identity.IsAuthenticated ? 1006 : 1007 });
            }

            return next();
        }
    }
}
