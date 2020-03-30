using System;
using Autofac;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.SharedKernel.Attributes;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.BusinessDomain.Admin
{
    [AutoRegister]
    public class DependencyModule : IDependencyModule
    {
        public DependencyModule()
        {
            //TODO: Add IOptions parameters
        }

        public void Register(IDependencyBuilder builder)
        {
            //Add registrations here
            builder.Autofac.RegisterType<DownloadableFileBusinessLogic>().As<IDownloadableFileBusinessLogic>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<AdminService>().As<IAdminService>().InstancePerLifetimeScope();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Add configuration here
        }
    }
}