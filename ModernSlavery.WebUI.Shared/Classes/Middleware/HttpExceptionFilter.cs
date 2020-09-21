using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebUI.Shared.Classes.Middleware
{
    public class HttpExceptionFilter : IActionFilter, IOrderedFilter
    {
        public int Order { get; set; } = int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is HttpException exception)
            {
                context.Result = new ObjectResult(exception.Value ?? exception.Message)
                {
                    StatusCode = (int)exception.StatusCode,
                };
                context.ExceptionHandled = true;
            }
        }
    }
}
