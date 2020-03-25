using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Routing;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Controllers;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace ModernSlavery.WebUI.Shared.Classes.Extensions
{
    public static partial class Extensions
    {
        public static string WithQuery(this IUrlHelper helper, string actionName, object routeValues)
        {
            var newRoute = new NameValueCollection();
            foreach (string key in helper.ActionContext.HttpContext.Request.Query.Keys)
            {
                newRoute[key] = helper.ActionContext.HttpContext.Request.Query[key];
            }

            foreach (KeyValuePair<string, object> item in new RouteValueDictionary(routeValues))
            {
                newRoute[item.Key] = item.Value.ToString();
            }

            string querystring = null;
            var keys = new SortedSet<string>(newRoute.AllKeys);
            foreach (string key in keys)
            {
                foreach (string value in newRoute.GetValues(key))
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(querystring))
                    {
                        querystring += "&";
                    }

                    querystring += $"{key}={value}";
                }
            }

            return helper.Action(actionName) + "?" + querystring;
        }

        public static void SetRoute(this IUrlHelper helper, RouteHelper.Routes route, string action, object values = null, string protocol = "https")
        {
            var routeValues = new RouteValueDictionary(values);
            helper.ActionContext.RouteData.Values.ForEach(rv => routeValues.Add(rv.Key, rv.Value));

            var controller =routeValues["Controller"].ToString();

            RouteHelper[route] = helper.Action(action, controller, values, protocol);
        }

        public static string Action(this IUrlHelper helper, RouteHelper.Routes route, object values=null, string protocol="https")
        {
            var routeValues = new RouteValueDictionary(values);
            helper.ActionContext.RouteData.Values.ForEach(rv=>routeValues.Add(rv.Key,rv.Value));

            var routeValue = RouteHelper.ContainsKey(route) ? RouteHelper[route] : null;
            if (string.IsNullOrWhiteSpace(routeValue)) throw new ArgumentException(nameof(route), "No route called '{route}' has not been declared. Use Url.SetRoute() to specify the route.");

            if (routeValue.StartsWithI("http:", "https:")) return routeValue;

            var routeInfo = RouteHelper.ResolveRoute(routeValue);
            if (!string.IsNullOrWhiteSpace(routeInfo.Area))routeValues.Add("Area",routeInfo.Area);

            if (string.IsNullOrWhiteSpace(routeInfo.Controller))
                return helper.Action(routeInfo.Action, routeValues["Controller"].ToString(), routeValues, protocol);

            return helper.Action(routeInfo.Action, GetControllerFriendlyName(routeInfo.Controller), routeValues, protocol);
        }

        public static string Action<TDestController>(this IUrlHelper helper, string action, object values, string protocol)
            where TDestController : BaseController
        {
            var routeValues = new RouteValueDictionary(values);
            AreaAttribute areaAttr = GetControllerArea<TDestController>();
            if (areaAttr != null)
            {
                routeValues.Add(areaAttr.RouteKey, areaAttr.RouteValue);
            }

            return helper.Action(action, GetControllerFriendlyName<TDestController>(), routeValues, protocol);
        }

        public static string Action<TDestController>(this IUrlHelper helper, string action, object values) where TDestController : BaseController
        {
            return helper.Action<TDestController>(action, new RouteValueDictionary(values), "https");
        }
        public static string Action<TDestController>(this IUrlHelper helper, string action)
            where TDestController : BaseController
        {
            return helper.Action<TDestController>(action, null);
        }
    }
}
