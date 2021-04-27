using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Models;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PreventDuplicatePostAttribute : ActionFilterAttribute
    {
        private readonly bool disableCache = true;

        public PreventDuplicatePostAttribute(bool disableCache = true)
        {
            this.disableCache = disableCache;
        }


        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (disableCache) context.HttpContext.DisableResponseCache();

            if (context.HttpContext.Request.Form.ContainsKey("__RequestVerificationToken"))
            {
                var currentToken = context.HttpContext.Request.Form["__RequestVerificationToken"];

                var session = context.HttpContext.RequestServices.GetService<IHttpSession>();

                var lastToken = session["LastRequestVerificationToken"];

                if (lastToken == currentToken)
                {
                    // return the custom error view
                    if (context.Controller is Controller controller)
                    {
                        var errorViewModelFactory = context.HttpContext.RequestServices.GetRequiredService<IErrorViewModelFactory>();

                        // create the session expired error model
                        var errorModel = errorViewModelFactory.Create(1150);

                        context.Result = controller.View("CustomError", errorModel);
                        return;
                    }
                    throw new HttpException(1150, "Duplicate post request");
                }

                session["LastRequestVerificationToken"] = currentToken;
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}