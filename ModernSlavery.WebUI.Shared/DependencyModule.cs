using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Models;
using ModernSlavery.WebUI.Shared.Services;

namespace ModernSlavery.WebUI.Shared
{
    public class DependencyModule: IDependencyModule
    {
        private readonly ILogger _logger;
        public DependencyModule(
            ILogger<DependencyModule> logger
        //TODO Add any required IOptions here
        )
        {
            _logger = logger;
            //TODO set any required local IOptions here
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //Register StaticAssetsVersioningHelper
            services.AddSingleton<StaticAssetsVersioningHelper>();

            //Add the custom url helper
            services.AddSingleton<IUrlHelperFactory,CustomUrlHelperFactory>();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            //Register HttpCache and HttpSession
            builder.RegisterType<HttpSession>().As<IHttpSession>().InstancePerLifetimeScope();
            builder.RegisterType<HttpCache>().As<IHttpCache>().SingleInstance();

            //Register the web service container
            builder.RegisterType<WebService>().As<IWebService>().InstancePerLifetimeScope();

            //Register factories
            builder.RegisterType<ErrorViewModelFactory>().As<IErrorViewModelFactory>()
                .SingleInstance();

            //Register Email queuers
            builder.RegisterType<SendEmailService>().As<ISendEmailService>().SingleInstance().WithAttributeFiltering();
            builder.RegisterType<NotificationService>().As<INotificationService>().SingleInstance().WithAttributeFiltering();

            ////Register all controllers - this is required to ensure KeyFilter is resolved in constructors
            //builder.RegisterAssemblyTypes(typeof(DependencyModule).Assembly)
            //    .Where(t => t.IsAssignableTo<Controller>())
            //    .InstancePerLifetimeScope()
            //    .WithAttributeFiltering();

        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Configure dependencies here
        }

        public void RegisterModules(IList<Type> modules)
        {
            //Register references dependency modules
            modules.Add(typeof(ModernSlavery.BusinessDomain.Shared.DependencyModule));
            modules.Add(typeof(ModernSlavery.WebUI.GDSDesignSystem.DependencyModule));

        }
    }
}
