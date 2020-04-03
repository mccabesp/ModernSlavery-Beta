using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Options;

namespace ModernSlavery.WebUI.Shared.Classes
{
    public class UrlRouteHelper : IUrlRouteHelper
    {
        private readonly UrlRouteOptions _routeOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UrlRouteHelper(UrlRouteOptions routeOptions, IHttpContextAccessor httpContextAccessor)
        {
            _routeOptions = routeOptions;
            _httpContextAccessor = httpContextAccessor;
        }

        public string Get(string routeName, object values = null)
        {
            var route = _routeOptions.ContainsKey(routeName) ? _routeOptions[routeName] : null;
            if (values != null) route = values.Resolve(route);
            if (route.IsRelativeUri()) route = _httpContextAccessor.HttpContext.ResolveUrl(route);
            return route;
        }

        public string Get(UrlRouteOptions.Routes routeType, object values = null)
        {
            return Get(routeType.GetAttribute<EnumMemberAttribute>().Value, values);
        }
    }
}