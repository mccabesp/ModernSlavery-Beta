using System;
using Autofac;
using ModernSlavery.Core.SharedKernel.Attributes;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.WebUI.Viewing.Presenters;

namespace ModernSlavery.WebUI.Viewing
{
    [AutoRegister]
    public class DependencyModule : IDependencyModule
    {
        public void Register(IDependencyBuilder builder)
        {
            //Register dependencies here
            builder.Autofac.RegisterType<ViewingPresenter>().As<IViewingPresenter>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<SearchPresenter>().As<ISearchPresenter>().InstancePerLifetimeScope();
            builder.Autofac.RegisterType<ComparePresenter>().As<IComparePresenter>()
                .InstancePerLifetimeScope();
        }

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
            //TODO: Configure dependencies here
        }
    }
}