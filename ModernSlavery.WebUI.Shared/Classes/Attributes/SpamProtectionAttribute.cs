using System;
using Microsoft.AspNetCore.Mvc.Filters;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    public class SpamProtectionAttribute : ActionFilterAttribute
    {
        private readonly int _minimumSeconds;
        public static SharedOptions SharedOptions;

        public SpamProtectionAttribute(int minimumSeconds = 10)
        {
            _minimumSeconds = minimumSeconds;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //If the model state isnt valid then return
            if (!filterContext.ModelState.IsValid) return;

            var remoteTime = DateTime.MinValue;

            try
            {
                remoteTime = Encryption.DecryptData(filterContext.HttpContext.GetParams("SpamProtectionTimeStamp"))
                    .FromSmallDateTime(true);
                if (remoteTime.AddSeconds(_minimumSeconds) < VirtualDateTime.Now) return;
            }
            catch
            {
            }

            if (SharedOptions.SkipSpamProtection) return;

            throw new HttpException(429, "Too Many Requests");
        }
    }
}