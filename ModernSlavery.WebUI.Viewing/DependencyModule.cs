using System;
using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.WebUI.Shared.Controllers;
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

        public void Register(IDependencyBuilder builder)
        {
            //Register references dependency modules
            builder.RegisterModule<ModernSlavery.BusinessDomain.Viewing.DependencyModule>();
            builder.RegisterModule<ModernSlavery.BusinessDomain.Shared.DependencyModule>();
            builder.RegisterModule<ModernSlavery.WebUI.Shared.DependencyModule>();

            //Register dependencies here
            builder.Autofac.RegisterType<ViewingPresenter>().As<IViewingPresenter>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<SearchPresenter>().As<ISearchPresenter>().InstancePerLifetimeScope();
            builder.Autofac.RegisterType<ComparePresenter>().As<IComparePresenter>()
                .InstancePerLifetimeScope();

            ////Register all controllers - this is required to ensure KeyFilter is resolved in constructors
            //builder.Autofac.RegisterAssemblyTypes(typeof(DependencyModule).Assembly)
            //    .Where(t => t.IsAssignableTo<Controller>())
            //    .InstancePerLifetimeScope()
            //    .WithAttributeFiltering();

        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Configure dependencies here
        }
    }
}