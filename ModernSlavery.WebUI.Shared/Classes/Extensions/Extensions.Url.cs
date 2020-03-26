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
using ModernSlavery.WebUI.Shared.Options;
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
