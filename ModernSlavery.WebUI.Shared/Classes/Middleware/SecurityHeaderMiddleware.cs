using System;
using System.Diagnostics;
using System.Security.Cryptography;
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

        //[DebuggerHidden()]
        public async Task Invoke(HttpContext httpContext)
        {
            //Create a nonce per request
            var nonce = CreateNonce();
            //Save the nonce to HttpContext for later inclusion on page
            httpContext.Items["nonce"] = nonce;

            httpContext.Response.OnStarting(
                () =>
                {
                    foreach (var key in _securityHeaders.Keys)
                    {
                        //Get the header value
                        var value = _securityHeaders[key];
                        //Replace all instances of the nonce
                        if (value.Contains("{nonce}", StringComparison.OrdinalIgnoreCase)) value = value.Replace("{nonce}", nonce,StringComparison.OrdinalIgnoreCase);
                        //Add the header
                        httpContext.SetResponseHeader(key, value);
                    }

                    return Task.CompletedTask;
                });

            await _next(httpContext);
        }

        private string CreateNonce()
        {
            var rng = new RNGCryptoServiceProvider();
            var nonceBytes = new byte[32];
            rng.GetBytes(nonceBytes);
            var nonce = Convert.ToBase64String(nonceBytes);
            return nonce;
        }
    }
}