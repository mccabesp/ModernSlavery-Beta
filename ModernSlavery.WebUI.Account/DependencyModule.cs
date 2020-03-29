using System;
using Autofac;
using ModernSlavery.Core.SharedKernel.Attributes;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.WebUI.Account.Interfaces;
using ModernSlavery.WebUI.Account.ViewServices;

namespace ModernSlavery.WebUI.Account
{
    [AutoRegister]
    public class DependencyModule: IDependencyModule
    {
        public DependencyModule()
        {
            //Any IOptions constructor parameters are automatically resolved
        }

        public void Register(IDependencyBuilder builder)
        {
            //Register dependencies here
            builder.Autofac.RegisterType<ChangeDetailsViewService>().As<IChangeDetailsViewService>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<ChangeEmailViewService>().As<IChangeEmailViewService>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<ChangePasswordViewService>().As<IChangePasswordViewService>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<CloseAccountViewService>().As<ICloseAccountViewService>()
                .InstancePerLifetimeScope();
        }

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
        }
    }
}
