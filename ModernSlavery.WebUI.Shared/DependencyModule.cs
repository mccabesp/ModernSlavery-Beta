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

        public void Register(IDependencyBuilder builder)
        {
            //Register references dependency modules
            builder.RegisterModule<ModernSlavery.BusinessDomain.Shared.DependencyModule>();
            builder.RegisterModule<ModernSlavery.WebUI.GDSDesignSystem.DependencyModule>();

            //Register HttpCache and HttpSession
            builder.Autofac.RegisterType<HttpSession>().As<IHttpSession>().InstancePerLifetimeScope();
            builder.Autofac.RegisterType<HttpCache>().As<IHttpCache>().SingleInstance();

            //Register the web service container
            builder.Autofac.RegisterType<WebService>().As<IWebService>().InstancePerLifetimeScope();

            //Register factories
            builder.Autofac.RegisterType<ErrorViewModelFactory>().As<IErrorViewModelFactory>()
                .SingleInstance();

            //Register Email queuers
            builder.Autofac.RegisterType<SendEmailService>().As<ISendEmailService>().SingleInstance().WithAttributeFiltering();
            builder.Autofac.RegisterType<NotificationService>().As<INotificationService>().SingleInstance().WithAttributeFiltering();

            ////Register all controllers - this is required to ensure KeyFilter is resolved in constructors
            //builder.Autofac.RegisterAssemblyTypes(typeof(DependencyModule).Assembly)
            //    .Where(t => t.IsAssignableTo<Controller>())
            //    .InstancePerLifetimeScope()
            //    .WithAttributeFiltering();

            //Register StaticAssetsVersioningHelper
            builder.Services.AddSingleton<StaticAssetsVersioningHelper>();

            //Add the custom url helper
            builder.Services.AddSingleton<IUrlHelperFactory,CustomUrlHelperFactory>();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
        }
    }
}
