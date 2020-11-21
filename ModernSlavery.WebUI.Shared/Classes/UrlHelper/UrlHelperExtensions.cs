using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;

namespace ModernSlavery.WebUI.Shared.Classes.UrlHelper
{
    public static class UrlHelperExtensions
    {
        public static string WithQuery(this IUrlHelper helper, string actionName, object routeValues)
        {
            var newRoute = new NameValueCollection();
            foreach (var key in helper.ActionContext.HttpContext.Request.Query.Keys)
                newRoute[key] = helper.ActionContext.HttpContext.Request.Query[key];

            foreach (var item in new RouteValueDictionary(routeValues)) newRoute[item.Key] = item.Value.ToString();

            string querystring = null;
            var keys = new SortedSet<string>(newRoute.AllKeys);
            foreach (var key in keys)
                foreach (var value in newRoute.GetValues(key))
                {
                    if (string.IsNullOrWhiteSpace(value)) continue;

                    foreach (var KeyValue in value.Split(','))
                    {
                        if (!string.IsNullOrWhiteSpace(querystring)) querystring += "&";
                        querystring += $"{key}={KeyValue}";
                    }
                }

            return helper.Action(actionName) + "?" + querystring;
        }

        public static string Action<TDestController>(this IUrlHelper helper, string action, object values,
            string protocol)
            where TDestController : BaseController
        {
            var routeValues = new RouteValueDictionary(values);
            var areaAttr = Extensions.Extensions.GetControllerArea<TDestController>();
            if (areaAttr != null) routeValues.Add(areaAttr.RouteKey, areaAttr.RouteValue);

            return helper.Action(action, Extensions.Extensions.GetControllerFriendlyName<TDestController>(), routeValues, protocol);
        }

        public static string Action<TDestController>(this IUrlHelper helper, string action, object values)
            where TDestController : BaseController
        {
            return helper.Action<TDestController>(action, new RouteValueDictionary(values), "https");
        }

        public static string Action<TDestController>(this IUrlHelper helper, string action)
            where TDestController : BaseController
        {
            return helper.Action<TDestController>(action, null);
        }

        public static string ActionArea(this IUrlHelper helper, string actionName, string controllerName, string areaName, object routeValues = null, string protocol = null, string host = null, string fragment = null)
        {
            if (string.IsNullOrWhiteSpace(actionName)) throw new ArgumentNullException(nameof(actionName));
            if (string.IsNullOrWhiteSpace(controllerName)) throw new ArgumentNullException(nameof(controllerName));
            if (string.IsNullOrWhiteSpace(areaName)) throw new ArgumentNullException(nameof(areaName));

            IDictionary<string, object> routeData = routeValues?.ToDynamic() ?? new Dictionary<string, object>();
            routeData["Area"] = areaName;
            return helper.Action(actionName, controllerName, routeData, protocol, host, fragment);
        }

        public static string PageArea(this IUrlHelper helper, string pageName, string areaName, string handlerName = null, object routeValues = null, string protocol = null, string host = null, string fragment = null)
        {
            if (string.IsNullOrWhiteSpace(pageName)) throw new ArgumentNullException(nameof(pageName));
            if (string.IsNullOrWhiteSpace(areaName)) throw new ArgumentNullException(nameof(areaName));

            IDictionary<string, object> routeData = routeValues?.ToDynamic() ?? new Dictionary<string, object>();
            routeData["Area"] = areaName;
            return helper.Page(pageName, handlerName, routeData, protocol, host, fragment);
        }

        public static bool IsAction(this IUrlHelper helper, string actionName, string controllerName = null, string areaName = null)
        {
            var currentUrl = helper.ActionContext.HttpContext.Request.Path.Value;
            var testUrl = string.IsNullOrWhiteSpace(areaName) ? helper.Action(actionName, controllerName) : helper.ActionArea(actionName, controllerName, areaName);
            return currentUrl.EqualsI(testUrl);
        }
    }
}