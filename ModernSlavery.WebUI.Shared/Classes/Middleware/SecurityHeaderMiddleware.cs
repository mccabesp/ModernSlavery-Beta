using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Options;

namespace ModernSlavery.WebUI.Shared.Classes.Middleware
{
    public class SecurityHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly SecurityHeaderOptions _securityHeaders;

        public SecurityHeaderMiddleware(RequestDelegate next, SecurityHeaderOptions securityHeaders)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _securityHeaders = securityHeaders ?? throw new ArgumentNullException(nameof(securityHeaders));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.OnStarting(
                () =>
                {
                    foreach (var key in _securityHeaders.Keys)
                    {
                        var value = _securityHeaders[key];

                        //Lookup the same value from another header
                        var varName = value.GetVariableName();
                        if (!string.IsNullOrWhiteSpace(varName) && _securityHeaders.ContainsKey(varName))
                            value = _securityHeaders[varName];

                        httpContext.SetResponseHeader(key, value);
                    }

                    return Task.CompletedTask;
                });

            await _next.Invoke(httpContext);
        }
    }
}