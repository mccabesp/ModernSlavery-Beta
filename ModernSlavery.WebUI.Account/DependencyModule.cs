using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Account.Interfaces;
using ModernSlavery.WebUI.Account.ViewServices;
using ModernSlavery.WebUI.Shared.Controllers;

namespace ModernSlavery.WebUI.Account
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
            //TODO: Register service dependencies here
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            //Register dependencies here
            builder.RegisterType<ChangeDetailsViewService>().As<IChangeDetailsViewService>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ChangeEmailViewService>().As<IChangeEmailViewService>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ChangePasswordViewService>().As<IChangePasswordViewService>()
                .InstancePerLifetimeScope();
            builder.RegisterType<CloseAccountViewService>().As<ICloseAccountViewService>()
                .InstancePerLifetimeScope();

            //Register all controllers - this is required to ensure KeyFilter is resolved in constructors
            builder.RegisterAssemblyTypes(typeof(DependencyModule).Assembly)
                .Where(t => t.IsAssignableTo<Controller>())
                .InstancePerLifetimeScope()
                .WithAttributeFiltering();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Configure dependencies here
        }

        public void RegisterModules(IList<Type> modules)
        {
            //Register references dependency modules
            modules.Add(typeof(ModernSlavery.BusinessDomain.Account.DependencyModule));
            modules.Add(typeof(ModernSlavery.BusinessDomain.Shared.DependencyModule));
            modules.Add(typeof(ModernSlavery.WebUI.Shared.DependencyModule));
        }
    }
}
