using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Admin.Classes;

namespace ModernSlavery.WebUI.Admin
{
    public class DependencyModule : IDependencyModule
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
            builder.RegisterType<AdminSearchService>().As<AdminSearchService>()
                .InstancePerLifetimeScope();
            builder.RegisterType<AdminHistory>().As<IAdminHistory>()
                .InstancePerLifetimeScope();
            builder.RegisterType<AuditLogger>().As<AuditLogger>()
                .InstancePerLifetimeScope();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Configure dependencies here
        }

        public void RegisterModules(IList<Type> modules)
        {
            //Register references dependency modules
            modules.AddDependency<ModernSlavery.BusinessDomain.Admin.DependencyModule>();
            modules.AddDependency<ModernSlavery.BusinessDomain.Shared.DependencyModule>();
            modules.AddDependency<ModernSlavery.WebUI.Shared.DependencyModule>();
        }
    }
}