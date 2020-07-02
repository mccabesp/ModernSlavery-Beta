using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ModernSlavery.Core.Interfaces
{
    /// <summary>
    ///     A module for containing all required dependencies requiring registration and associated configuration code to be
    ///     executed after all application dependencies have been built
    ///     Any IOptions parameters in constructors will be automatically resolved for use within Register or Configure methods
    ///     ///
    /// </summary>
    public interface IDependencyModule
    {
        /// <summary>
        ///     Place all your dependency service registrations in here 
        /// </summary>
        /// <param name="services">The service collection to Add your dependencies to</param>
        public void ConfigureServices(IServiceCollection services) { }

        /// <summary>
        ///     Place all your autofac service registrations in here 
        /// </summary>
        /// <param name="builder">The ContainerBuilder to Register your dependencies with</param>
        public void ConfigureContainer(ContainerBuilder builder) { }

        /// <summary>
        ///     Place any configuration of your services registered in "Configure" method.
        ///     Use lifetimeScope to resolve previously registered dependencies.
        /// </summary>
        /// <param name="lifetimeScope">The AutoFac scope for resolution of registered dependencies</param>
        public void Configure(ILifetimeScope lifetimeScope) { }

        public void RegisterModules(IList<Type> modules) { }
    }
}