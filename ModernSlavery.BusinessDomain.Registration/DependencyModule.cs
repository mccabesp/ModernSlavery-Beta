using System;
using Autofac;
using Autofac.Features.AttributeFilters;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.BusinessDomain.Registration
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
            builder.Autofac.RegisterType<OrganisationBusinessLogic>().As<IOrganisationBusinessLogic>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<SecurityCodeBusinessLogic>().As<ISecurityCodeBusinessLogic>()
                .SingleInstance();
            builder.Autofac.RegisterType<RegistrationService>().As<IRegistrationService>()
                .InstancePerLifetimeScope().WithAttributeFiltering();
            builder.Autofac.RegisterType<RegistrationBusinessLogic>().As<IRegistrationBusinessLogic>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<PinInThePostService>().As<PinInThePostService>().SingleInstance();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Add configuration here
        }
    }
}