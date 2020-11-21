using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Options;
using System.Collections.Generic;
using System.Linq;

namespace ModernSlavery.WebUI.Shared.Classes.UrlHelper
{
    public class CustomUrlHelper : IUrlHelper
    {
        private IUrlHelper _originalUrlHelper;
        private readonly StaticRoutesOptions _staticRoutesOptions;

        public ActionContext ActionContext { get; private set; }

        public CustomUrlHelper(ActionContext actionContext, IUrlHelper originalUrlHelper, StaticRoutesOptions staticRoutesOptions)
        {
            ActionContext = actionContext;
            _originalUrlHelper = originalUrlHelper;
            _staticRoutesOptions = staticRoutesOptions;
        }

        public string Action(UrlActionContext urlActionContext)
        {
            var result = _originalUrlHelper.Action(urlActionContext);
            if (!string.IsNullOrWhiteSpace(result)) return result;
            return FindRoute(urlActionContext);
        }

        public string Content(string contentPath)
        {
            return _originalUrlHelper.Content(contentPath);
        }

        public bool IsLocalUrl(string url)
        {
            return _originalUrlHelper.IsLocalUrl(url);
        }

        public string Link(string routeName, object values)
        {
            return _originalUrlHelper.Link(routeName, values);
        }

        public string RouteUrl(UrlRouteContext routeContext)
        {
            return _originalUrlHelper.RouteUrl(routeContext);
        }

        private string FindRoute(UrlActionContext urlActionContext)
        {
            if (!_staticRoutesOptions.Any()) return null;

            var keys = new List<string>();

            var area = urlActionContext?.Values?.GetProperty<string>("Area");
            if (string.IsNullOrWhiteSpace(area) && _originalUrlHelper.ActionContext.RouteData.Values.ContainsKey("area")) area = _originalUrlHelper.ActionContext.RouteData.Values["area"].ToString();
            if (!string.IsNullOrWhiteSpace(area)) keys.Add(area);

            var controller = urlActionContext.Controller;
            if (string.IsNullOrWhiteSpace(controller) && _originalUrlHelper.ActionContext.RouteData.Values.ContainsKey("controller")) controller = _originalUrlHelper.ActionContext.RouteData.Values["controller"].ToString();
            if (!string.IsNullOrWhiteSpace(controller)) keys.Add(controller);

            if (!string.IsNullOrWhiteSpace(urlActionContext.Action)) keys.Add(urlActionContext.Action);

            while (keys.Any())
            {
                var key = keys.ToDelimitedString(":");
                if (_staticRoutesOptions.ContainsKey(key))
                {
                    var value = _staticRoutesOptions[key];
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        if (value.StartsWith("~")) value = _originalUrlHelper.ActionContext.HttpContext.ResolveUrl(value);
                        if (urlActionContext.Values != null && value.Contains('{') && value.Contains('}')) value = urlActionContext.Values.Resolve(value);
                        return value;
                    }
                }
                keys.RemoveAt(0);
            }

            return null;
        }
    }
}
