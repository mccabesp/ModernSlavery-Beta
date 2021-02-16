using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Shared.Classes
{
    public abstract class BaseRazorView<TModel> : RazorPage<TModel>
    {
        [RazorInject]
        protected ISharedBusinessLogic SharedBusinessLogic { get; set; }

        [RazorInject]
        protected IHttpSession Session { get; set; }

        protected BaseRazorView()
        {

        }

        public string ActionName => ViewContext.RouteData.Values["action"].ToString();

        public string ControllerName => ViewContext.RouteData.Values["controller"].ToString();

        public string AreaName => ViewContext.RouteData.Values.ContainsKey("area") ? ViewContext.RouteData.Values["area"].ToString() : null;


        protected BaseController Controller => ViewContext.ViewData["controller"] as BaseController;

        protected string Title { get => ViewData.ContainsKey("Title") ? ViewData["Title"].ToString() : null; set => ViewData["Title"] = value; }
        protected string Subtitle { get => ViewData.ContainsKey("Subtitle") ? ViewData["Subtitle"].ToString() : null; set => ViewData["Subtitle"] = value; }
        protected string Description { get => ViewData.ContainsKey("MetaDescription") ? ViewData["MetaDescription"].ToString() : null; set => ViewData["MetaDescription"] = value; }
        protected RobotDirectives Robots { get => ViewData.ContainsKey("MetaRobots") ? (RobotDirectives)ViewData["MetaRobots"] : RobotDirectives.None; set => ViewData["MetaRobots"] = value; }
        protected Dictionary<string, string> OpenGraph {
            get {
                if (!ViewData.ContainsKey("MetaOpenGraph")) ViewData["MetaOpenGraph"] = new Dictionary<string, string>();
                return (Dictionary<string, string>)ViewData["MetaOpenGraph"];
            }
        }

#if RELEASE
        public const bool IsDebug = false;
        public const bool IsRelease = true;
#else
        public const bool IsDebug = true;
        public const bool IsRelease = false;
#endif

        public string UserHostAddress => Context.GetUserHostAddress();

        public bool IsTrustedIP => SharedBusinessLogic.SharedOptions.IsTrustedAddress(UserHostAddress);

        public bool IsAdministrator => SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(Controller.VirtualUser);
        public bool IsSuperAdministrator => IsTrustedIP && SharedBusinessLogic.AuthorisationBusinessLogic.IsSuperAdministrator(Controller.VirtualUser);
        public bool IsDatabaseAdministrator => IsTrustedIP && SharedBusinessLogic.AuthorisationBusinessLogic.IsDatabaseAdministrator(Controller.VirtualUser);
        public bool IsDevOpsAdministrator => IsTrustedIP && SharedBusinessLogic.AuthorisationBusinessLogic.IsDevOpsAdministrator(Controller.VirtualUser);

        protected List<string> GetDisplayMessages()
        {
            var messages = Session.Get<List<string>>("DisplayMessages") ?? new List<string>();
            return messages;
        }
        protected void ClearDisplayMessages()
        {
            Session.Remove("DisplayMessages");
        }
    }
    public enum RobotDirectives
    {
        None=0,
        NoIndex,
        NoFollow,
        All
    }

}
