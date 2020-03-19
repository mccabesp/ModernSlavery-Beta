using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ModernSlavery.Extensions;
using ModernSlavery.WebUI.Shared.Options;

namespace ModernSlavery.WebUI.Shared.Classes.Middleware
{
    public class SecurityHeaderMiddleware
    {

        private readonly RequestDelegate _next;

        public SecurityHeaderMiddleware(RequestDelegate next, SecurityHeaderOptions securityHeaders)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _securityHeaders = securityHeaders ?? throw new ArgumentNullException(nameof(securityHeaders));
        }

        private readonly SecurityHeaderOptions _securityHeaders;

        public async Task Invoke(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            httpContext.Response.OnStarting(
                () => {
                    foreach (string key in _securityHeaders.Keys)
                    {
                        string value = _securityHeaders[key];

                        //Lookup the same value from another header
                        string varName = value.GetVariableName();
                        if (!string.IsNullOrWhiteSpace(varName) && _securityHeaders.ContainsKey(varName))
                        {
                            value = _securityHeaders[varName];
                        }

                        httpContext.SetResponseHeader(key, value);
                    }

                    return Task.CompletedTask;
                });

            await _next.Invoke(httpContext);
        }
    }
}
