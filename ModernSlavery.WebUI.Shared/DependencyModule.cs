using System;
using Autofac;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Shared
{
    public class DependencyModule: IDependencyModule
    {
        public DependencyModule()
        {
            //Any IOptions constructor parameters are automatically resolved
        }

        public bool AutoSetup { get; } = true;

        public void Register(IDependencyBuilder builder)
        {
            //Register HttpCache and HttpSession
            builder.ContainerBuilder.RegisterType<HttpSession>().As<IHttpSession>().InstancePerLifetimeScope();
            builder.ContainerBuilder.RegisterType<HttpCache>().As<IHttpCache>().SingleInstance();

            //Register Url route helper
            builder.ServiceCollection.AddSingleton<UrlRouteHelper>(); 
            
            //Register the web service container
            builder.ServiceCollection.AddSingleton<WebService>();
        }

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
        }
    }
}
