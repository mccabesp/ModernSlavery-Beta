using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Submission.Controllers;

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
            //Configure dependencies here

            //Set the submission home url 
            var urlHelper = container.Resolve<IUrlHelper>();
            urlHelper.SetRoute(RouteHelper.Routes.SubmissionHome, nameof(SubmissionController.ManageOrganisations));

            //Set the scope home url            
            urlHelper.SetRoute(RouteHelper.Routes.ScopeHome, nameof(ScopeController.DeclareScope));
        }
    }
}
