using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    public class SpamProtectionAttribute : ActionFilterAttribute
    {
        private readonly int _minimumSeconds;

        public SpamProtectionAttribute(int minimumSeconds = 10)
        {
            _minimumSeconds = minimumSeconds;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //If the model state isnt valid then return
            if (!filterContext.ModelState.IsValid) return;

            try
            {
                var remoteTime = Encryption.DecryptData(filterContext.HttpContext.GetParams("SpamProtectionTimeStamp"))
                    .FromSmallDateTime(true);
                if (remoteTime.AddSeconds(_minimumSeconds) < VirtualDateTime.Now) return;
            }
            catch
            {
            }

            var testOptions = filterContext.HttpContext.RequestServices.GetRequiredService<TestOptions>();
            if (testOptions.SkipSpamProtection) return;

            throw new HttpException(429, "Too Many Requests");
        }
    }
}