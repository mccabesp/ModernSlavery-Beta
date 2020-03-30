using System;
using Autofac;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.SharedKernel.Attributes;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.WebUI.Account.Interfaces;
using ModernSlavery.WebUI.Account.ViewServices;

namespace ModernSlavery.WebUI.Account
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

        public void Configure(ILifetimeScope lifetimeScope)
        {
        }
    }
}
