using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    public class ApiControllerExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private static ILogger _logger;

        public override void OnException(ExceptionContext context)
        {
            var hex = context.Exception as HttpException;
            if (hex != null)
            {
                _logger??=context.HttpContext.RequestServices.GetRequiredService<ILogger<ApiControllerExceptionFilterAttribute>>();
                _logger.LogWarning(hex, hex.Message);
                context.Result = new StatusCodeResult((int)hex.StatusCode);

                context.ExceptionHandled = true;
            }
        }
    }
}