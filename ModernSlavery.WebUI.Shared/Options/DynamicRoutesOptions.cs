using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Options;

namespace ModernSlavery.WebUI.Shared.Options
{
    [Options("DynamicRoutes", true)]
    public class DynamicRoutesOptions : Dictionary<string, string>, IOptions
    {
        public DynamicRoutesOptions() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public void MapDynamicRoutes(IEndpointRouteBuilder endpoints)
        {
            foreach (var key in this.Keys)
            {
                if (this[key].StartsWith('/'))
                {
                    endpoints.MapFallbackToPage(key, this[key]);
                    continue;
                }
                var parts = this[key].SplitI(":");
                var actionName = parts.Length > 0 ? parts[0] : null;
                var controllerName = parts.Length > 1 ? parts[1] : null;
                var areaName = parts.Length > 2 ? parts[2] : null;
                if (string.IsNullOrWhiteSpace(areaName))
                    endpoints.MapFallbackToController(key, actionName, controllerName);
                else
                    endpoints.MapFallbackToAreaController(key, actionName, controllerName, areaName);
            }
        }
    }
}