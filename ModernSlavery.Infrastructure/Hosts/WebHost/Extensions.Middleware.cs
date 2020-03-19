using Microsoft.AspNetCore.Builder;
using ModernSlavery.WebUI.Shared.Classes.Middleware;

namespace ModernSlavery.Infrastructure.Hosts.WebHost
{
    public static partial class Extensions
    {
        public static IApplicationBuilder UseSecurityHeaderMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeaderMiddleware>();
        }

        public static IApplicationBuilder UseStickySessionMiddleware(this IApplicationBuilder builder, bool enable)
        {
            return builder.UseMiddleware<StickySessionMiddleware>(enable);
        }

        public static IApplicationBuilder UseMaintenancePageMiddleware(this IApplicationBuilder builder, bool enable)
        {
            return builder.UseMiddleware<MaintenancePageMiddleware>(enable);
        }
    }
}