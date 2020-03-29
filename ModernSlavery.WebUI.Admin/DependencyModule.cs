using System;
using Autofac;
using ModernSlavery.Core.SharedKernel.Attributes;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.WebUI.Admin.Classes;

namespace ModernSlavery.WebUI.Admin
{
    [AutoRegister]
    public class DependencyModule : IDependencyModule
    {
        public void Register(IDependencyBuilder builder)
        {
            //Register dependencies here
            builder.Autofac.RegisterType<AdminSearchService>().As<AdminSearchService>()
                .InstancePerLifetimeScope();
        }

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
            //TODO: Configure dependencies here
        }
    }
}