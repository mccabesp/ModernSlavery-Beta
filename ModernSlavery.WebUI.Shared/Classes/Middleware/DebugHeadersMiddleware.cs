using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.WebUI.Shared.Classes.Middleware
{
    public class DebugHeadersMiddleware
    {
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;

        public DebugHeadersMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _logger = logger;
            _next = next;
        }

        [DebuggerHidden()]
        public async Task InvokeAsync(HttpContext httpContext)
        {
            // Request method, scheme, and path
            _logger.LogDebug($"Request Method: {httpContext.Request.Method} Scheme: {httpContext.Request.Scheme} Path: {httpContext.Request.Path}");

            // Headers
            foreach (var header in httpContext.Request.Headers)
            {
                _logger.LogDebug($"Header: {header.Key}: {header.Value}");
            }

            // Connection: RemoteIp
            _logger.LogDebug($"Request RemoteIp: {httpContext.Connection.RemoteIpAddress}");
            _logger.LogDebug($"Request UserHostAddress: {httpContext.GetUserHostAddress()}");

            await _next(httpContext);
        }
    }
}