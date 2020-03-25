using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Viewing.Controllers;

namespace ModernSlavery.WebUI.Account
{
    public class DependencyModule: IDependencyModule
    {
        public DependencyModule()
        {
            //Any IOptions constructor parameters are automatically resolved
        }

        public bool AutoSetup { get; } = false;

        public void Register(DependencyBuilder builder)
        {
            //TODO: Register dependencies here
        }

        public void Configure(IContainer container)
        {
            //Set the search home url   
            var urlHelper = container.Resolve<IUrlHelper>();
            urlHelper.SetRoute(RouteHelper.Routes.ViewingHome, nameof(ViewingController.Index));
            urlHelper.SetRoute(RouteHelper.Routes.SearchHome, nameof(ViewingController.SearchResults));
            urlHelper.SetRoute(RouteHelper.Routes.ViewingDownloads, nameof(ViewingController.Download)); 
            urlHelper.SetRoute(RouteHelper.Routes.ViewingGuidance, nameof(ReportingStepByStepController.StepByStepStandalone));
            urlHelper.SetRoute(RouteHelper.Routes.ViewingActionHub, nameof(ActionHubController.Overview));
        }
    }
}
