using System;
using Microsoft.AspNetCore.Mvc.Filters;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    public class SpamProtectionAttribute : ActionFilterAttribute
    {
        private readonly int _minimumSeconds;
        private readonly SharedOptions _sharedOptions;

        public SpamProtectionAttribute(int minimumSeconds = 10)
        {
            _sharedOptions = Activator.CreateInstance<SharedOptions>();

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

            if (_sharedOptions.SkipSpamProtection) return;

            throw new HttpException(429, "Too Many Requests");
        }
    }
}