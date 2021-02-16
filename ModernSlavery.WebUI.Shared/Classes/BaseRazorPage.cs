using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Shared.Classes
{
    //
    // Summary:
    //     A base class for a Razor page.
    public abstract class BaseRazorPage : PageBase
    {
        [RazorInject]
        protected ISharedBusinessLogic SharedBusinessLogic { get; set; }

        [RazorInject]
        protected IHttpSession Session { get; set; }

        public BaseRazorPage()
        {
        }

        protected string Title { get => ViewContext.ViewData.ContainsKey("Title") ? ViewContext.ViewData["Title"].ToString() : null; set => ViewContext.ViewData["Title"] = value; }
        protected string Subtitle { get => ViewContext.ViewData.ContainsKey("Subtitle") ? ViewContext.ViewData["Subtitle"].ToString() : null; set => ViewContext.ViewData["Subtitle"] = value; }
        protected string Description { get => ViewContext.ViewData.ContainsKey("MetaDescription") ? ViewContext.ViewData["MetaDescription"].ToString() : null; set => ViewContext.ViewData["MetaDescription"] = value; }
        protected RobotDirectives Robots { get => ViewContext.ViewData.ContainsKey("MetaRobots") ? (RobotDirectives)ViewContext.ViewData["MetaRobots"] : RobotDirectives.None; set => ViewContext.ViewData["MetaRobots"] = value; }
        protected Dictionary<string, string> OpenGraph {
            get {
                if (!ViewContext.ViewData.ContainsKey("MetaOpenGraph")) ViewContext.ViewData["MetaOpenGraph"] = new Dictionary<string, string>();
                return (Dictionary<string, string>)ViewContext.ViewData["MetaOpenGraph"];
            }
        }


        public string UserHostAddress => HttpContext.GetUserHostAddress();

        public bool IsTrustedIP => SharedBusinessLogic.SharedOptions.IsTrustedAddress(UserHostAddress);

        public bool IsAdministrator => SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(User);
        public bool IsSuperAdministrator => IsTrustedIP && SharedBusinessLogic.AuthorisationBusinessLogic.IsSuperAdministrator(User);
        public bool IsDatabaseAdministrator => IsTrustedIP && SharedBusinessLogic.AuthorisationBusinessLogic.IsDatabaseAdministrator(User);
        public bool IsDevOpsAdministrator => IsTrustedIP && SharedBusinessLogic.AuthorisationBusinessLogic.IsDevOpsAdministrator(User);

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
}
