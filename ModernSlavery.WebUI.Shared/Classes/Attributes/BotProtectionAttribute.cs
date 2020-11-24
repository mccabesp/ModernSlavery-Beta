using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    public class BotProtectionAttribute : ActionFilterAttribute
    {
        private readonly int _minimumSeconds;

        public BotProtectionAttribute(int minimumSeconds = 10)
        {
            _minimumSeconds = minimumSeconds;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //If the model state isnt valid then return
            if (!filterContext.ModelState.IsValid) return;

            try
            {
                var remoteTime = Encryption.DecryptData(filterContext.HttpContext.GetParams("BotProtectionTimeStamp")).FromSmallDateTime(true);
                if (remoteTime.AddSeconds(_minimumSeconds) < VirtualDateTime.Now) return;
            }
            catch
            {
            }

            var testOptions = filterContext.HttpContext.RequestServices.GetRequiredService<TestOptions>();

            if (!testOptions.DisableLockoutProtection) throw new HttpException(429, "Too Many Requests");
        }
    }
}