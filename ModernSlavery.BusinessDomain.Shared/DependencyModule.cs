using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.BusinessDomain.Shared.Classes;
using ModernSlavery.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace ModernSlavery.BusinessDomain.Shared
{
    public class DependencyModule : IDependencyModule
    {
        public DependencyModule()
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //TODO: Register service dependencies here
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            //Add registrations here
            builder.RegisterType<SharedBusinessLogic>().As<ISharedBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<UpdateFromCompaniesHouseService>().As<UpdateFromCompaniesHouseService>().InstancePerLifetimeScope();

            //Register Email queuers
            builder.RegisterType<SendEmailService>().As<ISendEmailService>().SingleInstance().WithAttributeFiltering();
            builder.RegisterType<NotificationService>().As<INotificationService>().SingleInstance().WithAttributeFiltering();
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