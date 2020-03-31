using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Options;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class FeatureSwitchAttribute : ActionFilterAttribute
    {
        private readonly string _featureName;

        public FeatureSwitchAttribute(string featureName)
        {
            if (string.IsNullOrWhiteSpace(featureName)) throw new ArgumentNullException(nameof(featureName));
            _featureName = featureName;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var options =(FeatureSwitchOptions) context.HttpContext.RequestServices.GetRequiredService(typeof(FeatureSwitchOptions));

            if (options.IsDisabled(_featureName)) context.Result = new HttpStatusCodeResult(HttpStatusCode.NotFound);

            base.OnActionExecuting(context);
        }
    }
}