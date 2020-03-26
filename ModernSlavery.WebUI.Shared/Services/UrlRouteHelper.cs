using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Distributed;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Options;

namespace ModernSlavery.WebUI.Shared.Classes
{
    public class UrlRouteHelper : IUrlRouteHelper
    {
        private readonly UrlRouteOptions _routeOptions;
        private readonly IDistributedCache _cache;
        private IHttpContextAccessor _httpContextAccessor;
        public UrlRouteHelper(UrlRouteOptions routeOptions, IDistributedCache cache, IHttpContextAccessor httpContextAccessor)
        {
            _routeOptions = routeOptions;
            _cache = cache;
            _httpContextAccessor=httpContextAccessor;
        }

        public async Task<string> Get(string routeName)
        {
            var route = _routeOptions.ContainsKey(routeName) ? _routeOptions[routeName] : null;
            if (string.IsNullOrWhiteSpace(route))route = await _cache.GetStringAsync($"UrlRoutes:{routeName}");
            if (route.IsRelativeUri())route= _httpContextAccessor.HttpContext.ResolveUrl(route);
            return route;
        }

        public async Task<string> Get(UrlRouteOptions.Routes routeType)
        {
            return await Get(routeType.GetAttribute<EnumMemberAttribute>().Value);
        }

        public async Task Set(string routeName, string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                if (_routeOptions.ContainsKey(routeName))_routeOptions.Remove(routeName);
                await _cache.RemoveAsync($"UrlRoutes:{routeName}");
            }
            else
            {
                _routeOptions[routeName] = url;
                await _cache.SetStringAsync($"UrlRoutes:{routeName}", url);
            }
        }
        public async Task Set(UrlRouteOptions.Routes routeType, string url)
        {
            await Set(routeType.GetAttribute<EnumMemberAttribute>().Value,url);
        }
    }
}
