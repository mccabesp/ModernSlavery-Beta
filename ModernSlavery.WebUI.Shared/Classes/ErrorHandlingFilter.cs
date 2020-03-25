using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Shared.Classes
{
    public class ErrorHandlingFilter : ExceptionFilterAttribute
    {

        private readonly ILogger _logger;

        public ErrorHandlingFilter(ILogger<ErrorHandlingFilter> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            var hex = context.Exception as HttpException;
            if (hex != null)
            {
                _logger.LogWarning(hex, hex.Message);
                var webService=(IWebService)context.HttpContext.RequestServices.GetService(typeof(IWebService));
                context.Result = new ViewResult {
                    ViewName = "CustomError",
                    ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), context.ModelState) {
                        Model = webService.ErrorViewModelFactory.Create(hex.StatusCode) // set the model
                    }
                };

                context.ExceptionHandled = true;
            }
        }

    }
}
