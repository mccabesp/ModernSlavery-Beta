using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    public class ControllerExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private static ILogger _logger;

        public override void OnException(ExceptionContext context)
        {
            var hex = context.Exception as HttpException;
            if (hex != null)
            {
                _logger ??= context.HttpContext.RequestServices.GetRequiredService<ILogger<ControllerExceptionFilterAttribute>>();
                _logger.LogWarning(hex, hex.Message);
                context.Result = new RedirectToActionResult("Default", "Error", new { errorCode = (int)hex.StatusCode });
                context.ExceptionHandled = true;
            }
        }
    }
}