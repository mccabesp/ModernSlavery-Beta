using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.WebUI.Shared.Classes
{
    public class RouteHelper: Dictionary<RouteHelper.Routes, string>
    {
        public enum Routes
        {
            AccountSignOut, 
            AccountHome,
            AdminHome,
            RegistrationHome,
            ScopeHome, 
            SubmissionHome,
            SearchHome,
            ViewingActionHub,
            ViewingDownloads,
            ViewingGuidance,
            ViewingHome
        }

        public (string Action, string Controller, string Area) ResolveRoute(string routeValue)
        {
            var routeParts = routeValue.SplitI(":-,;. ");

            var action = routeParts.Length > 0 ? routeParts[0] : null;
            if (string.IsNullOrWhiteSpace(action)) throw new Exception("Route '{route}' must contain an action in format 'Action[:Controller][:Area]'");
            if (routeParts.Length>3) throw new Exception("Route '{route}' contains too many parts it must be specified in format 'Action[:Controller][:Area]'");
            var area=routeParts.Length>2 ? routeParts[2] : null;
            var controller=routeParts.Length>1 ? routeParts[1] : null;

            return (action, controller, area);
        }
    }
}
