using System;
using Autofac;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.SharedKernel.Attributes;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.BusinessDomain.Registration
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
            builder.Autofac.RegisterType<OrganisationBusinessLogic>().As<IOrganisationBusinessLogic>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<SecurityCodeBusinessLogic>().As<ISecurityCodeBusinessLogic>()
                .SingleInstance();
            builder.Autofac.RegisterType<RegistrationService>().As<IRegistrationService>()
                .InstancePerLifetimeScope();

            builder.Autofac.RegisterType<PinInThePostService>().As<PinInThePostService>().SingleInstance();
        }

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
            //TODO: Add configuration here
        }
    }
}