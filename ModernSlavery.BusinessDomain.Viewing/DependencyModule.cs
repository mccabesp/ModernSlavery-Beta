using System;
using Autofac;
using Autofac.Features.AttributeFilters;
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

        public void Register(IDependencyBuilder builder)
        {
            //Add registrations here
            builder.Autofac.RegisterType<SearchBusinessLogic>().As<ISearchBusinessLogic>().SingleInstance()
                .WithAttributeFiltering();
            builder.Autofac.RegisterType<ViewingService>().As<IViewingService>().SingleInstance();
            builder.Autofac.RegisterType<CompareBusinessLogic>().As<ICompareBusinessLogic>()
                .InstancePerLifetimeScope();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Add configuration here
        }
    }
}