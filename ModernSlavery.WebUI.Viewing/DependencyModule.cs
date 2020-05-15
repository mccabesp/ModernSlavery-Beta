using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Viewing.Presenters;

namespace ModernSlavery.WebUI.Viewing
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

        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            //Register dependencies here
            builder.RegisterType<ViewingPresenter>().As<IViewingPresenter>()
                .InstancePerLifetimeScope();
            builder.RegisterType<SearchPresenter>().As<ISearchPresenter>().InstancePerLifetimeScope();
            builder.RegisterType<ComparePresenter>().As<IComparePresenter>()
                .InstancePerLifetimeScope();
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
            //TODO: Add any linked dependency modules here
            modules.Add(typeof(ModernSlavery.BusinessDomain.Viewing.DependencyModule));
            modules.Add(typeof(ModernSlavery.BusinessDomain.Shared.DependencyModule));
            modules.Add(typeof(ModernSlavery.WebUI.Shared.DependencyModule));
        }
    }
}