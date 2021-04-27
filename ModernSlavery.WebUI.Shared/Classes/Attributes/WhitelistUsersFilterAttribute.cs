using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method,Inherited = true)]
    public class WhitelistUsersFilterAttribute : ActionFilterAttribute
    {
        private readonly string[] _actionExemptions;
        public WhitelistUsersFilterAttribute(params string[] actionExemptions)
        {
            _actionExemptions = actionExemptions;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                bool skip = false;
                if (context.RouteData.Values.ContainsKey("action"))
                {
                    var action = context.RouteData.Values["action"];
                    skip = _actionExemptions.Any(a => action.EqualsI(a));
                }
                if (!skip) 
                {
                    var testOptions = context.HttpContext.RequestServices.GetRequiredService<TestOptions>();
                    if (testOptions.WhitelistingEnabled)
                    {
                        context.Result = new ChallengeResult();
                        return;
                    } 
                }
            }
            base.OnActionExecuting(context);
        }
    }
}