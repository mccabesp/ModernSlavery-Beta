using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.BusinessDomain.Account
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
            builder.RegisterType<AuthorisationBusinessLogic>().As<IAuthorisationBusinessLogic>().SingleInstance();
            builder.RegisterType<AuthenticationBusinessLogic>().As<IAuthenticationBusinessLogic>().SingleInstance();
            builder.RegisterType<AccountService>().As<IAccountService>().SingleInstance();
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