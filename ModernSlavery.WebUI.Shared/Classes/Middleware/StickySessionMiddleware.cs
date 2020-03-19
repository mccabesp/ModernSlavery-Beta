using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ModernSlavery.WebUI.Shared.Classes.Middleware
{
    public class StickySessionMiddleware
    {

        private readonly bool _disable;
        private readonly RequestDelegate _next;

        public StickySessionMiddleware(RequestDelegate next, bool enable)
        {
            _next = next;
            _disable = !enable;
        }

        public async Task Invoke(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            httpContext.Response.OnStarting(
                () => {
                    //Disable sticky sessions
                    httpContext.SetResponseHeader("Arr-Disable-Session-Affinity", _disable.ToString());

                    return Task.CompletedTask;
                });

            await _next.Invoke(httpContext);
        }

    }
}
