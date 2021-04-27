using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Extensions;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    public class ControllerExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private static ILogger _logger;

        public override void OnException(ExceptionContext context)
        {
            _logger ??= context.HttpContext.RequestServices.GetRequiredService<ILogger<ControllerExceptionFilterAttribute>>();
            int errorCode = 500;
            if (context.Exception is HttpException hex)
            {
                _logger.LogWarning(hex, hex.Message);
                errorCode = (int)hex.StatusCode;
            }
            else
            {
                _logger.LogError(context.Exception, context.Exception.Message);
            }

            context.Result = context.RouteData.GetRedirectToErrorPageResult(errorCode);

            context.ExceptionHandled = true;
        }
    }
}