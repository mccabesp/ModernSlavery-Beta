using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Interfaces;

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
            if (disableCache)
            {
                context.HttpContext.DisableResponseCache();
            }

            if (context.HttpContext.Request.Form.ContainsKey("__RequestVerificationToken"))
            {
                StringValues currentToken = context.HttpContext.Request.Form["__RequestVerificationToken"];

                var session = context.HttpContext.RequestServices.GetService<IHttpSession>();

                object lastToken = session["LastRequestVerificationToken"];

                if (lastToken == currentToken)
                {
                    throw new HttpException(1150, "Duplicate post request");
                }

                session["LastRequestVerificationToken"] = currentToken;
            }

            await base.OnActionExecutionAsync(context, next);
        }

    }
}
