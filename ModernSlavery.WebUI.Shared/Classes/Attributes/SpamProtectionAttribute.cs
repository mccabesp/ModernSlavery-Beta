using System;
using System.Web;
using ModernSlavery.Core;
using ModernSlavery.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using ModernSlavery.SharedKernel.Options;

namespace ModernSlavery.WebUI.Shared.Classes
{
    public class SpamProtectionAttribute : ActionFilterAttribute
    {
        private GlobalOptions _globalOptions;

        private readonly int _minimumSeconds;

        public SpamProtectionAttribute(int minimumSeconds = 10)
        {
            _globalOptions = Activator.CreateInstance<GlobalOptions>();

            _minimumSeconds = minimumSeconds;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //If the model state isnt valid then return
            if (!filterContext.ModelState.IsValid)
            {
                return;
            }

            DateTime remoteTime = DateTime.MinValue;

            try
            {
                remoteTime = Encryption.DecryptData(filterContext.HttpContext.GetParams("SpamProtectionTimeStamp")).FromSmallDateTime(true);
                if (remoteTime.AddSeconds(_minimumSeconds) < VirtualDateTime.Now)
                {
                    return;
                }
            }
            catch { }

            if (_globalOptions.SkipSpamProtection)
            {
                return;
            }

            throw new HttpException(429, "Too Many Requests");
        }

    }
}
