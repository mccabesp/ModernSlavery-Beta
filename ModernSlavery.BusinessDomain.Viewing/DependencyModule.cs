using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.BusinessDomain.Viewing
{
    public class DependencyModule : IDependencyModule
    {
        public DependencyModule()
        {
            //TODO: Add IOptions parameters
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //TODO: Register service dependencies here
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            //Add registrations here
            builder.RegisterType<SearchBusinessLogic>().As<ISearchBusinessLogic>().InstancePerLifetimeScope().WithAttributeFiltering();
            builder.RegisterType<ViewingService>().As<IViewingService>().InstancePerLifetimeScope();
            builder.RegisterType<CompareBusinessLogic>().As<ICompareBusinessLogic>().InstancePerLifetimeScope();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Configure dependencies here
        }

        public void RegisterModules(IList<Type> modules)
        {
            //TODO: Add any linked dependency modules here
        }
    }
}