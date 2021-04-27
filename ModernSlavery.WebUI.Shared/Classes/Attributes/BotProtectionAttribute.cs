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

            var timeSpent = TimeSpan.Zero;
            try
            {
                var remoteTime = Encryption.Decrypt(filterContext.HttpContext.GetParams("BotProtectionTimeStamp"), Encryption.Encodings.Base62).FromSmallDateTime(true);
                timeSpent = VirtualDateTime.Now - remoteTime;
                if (timeSpent.TotalSeconds>=_minimumSeconds) return;
            }
            catch
            {
            }

            var testOptions = filterContext.HttpContext.RequestServices.GetRequiredService<TestOptions>();

            if (!testOptions.DisableLockoutProtection) throw new HttpException(429, $"Postback to '{filterContext.HttpContext.GetUri().PathAndQuery} took {timeSpent.TotalSeconds}' which did not exceeded minimum of {_minimumSeconds} seconds");
        }
    }
}