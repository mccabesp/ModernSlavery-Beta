using System;
using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel.Attributes;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Models;
using ModernSlavery.WebUI.Shared.Services;

namespace ModernSlavery.WebUI.Shared
{
    [AutoRegister]
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
            //Register HttpCache and HttpSession
            builder.Autofac.RegisterType<HttpSession>().As<IHttpSession>().InstancePerLifetimeScope();
            builder.Autofac.RegisterType<HttpCache>().As<IHttpCache>().SingleInstance();

            //Register Url route helper
            builder.Services.AddSingleton<UrlRouteHelper>(); 
            
            //Register the web service container
            builder.Services.AddSingleton<WebService>();

            //Register factories
            builder.Autofac.RegisterType<ErrorViewModelFactory>().As<IErrorViewModelFactory>()
                .SingleInstance();

            //Register Email queuers
            builder.Autofac.RegisterType<SendEmailService>().As<ISendEmailService>().SingleInstance();
            builder.Autofac.RegisterType<NotificationService>().As<INotificationService>().SingleInstance();

            //Register all controllers - this is required to ensure KeyFilter is resolved in constructors
            builder.Autofac.RegisterAssemblyTypes(typeof(BaseController).Assembly)
                .Where(t => t.IsAssignableTo<BaseController>())
                .InstancePerLifetimeScope()
                .WithAttributeFiltering();

            //Register StaticAssetsVersioningHelper
            builder.Services.AddSingleton<StaticAssetsVersioningHelper>();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
        }
    }
}
