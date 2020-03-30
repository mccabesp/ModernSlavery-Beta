using System;
using Autofac;

namespace ModernSlavery.Core.SharedKernel.Interfaces
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
        ///     Place all your dependency registrations in here using builder.ContainerBuilder (Autofac IContainer) or
        ///     builder.Services (IServiceCollection)
        ///     You can also register other modules using builder.Register<TModule>()
        /// </summary>
        /// <param name="builder">This is the instance of DependencyBuilder for the application host</param>
        void Register(IDependencyBuilder builder);

        /// <summary>
        ///     Place any configuration of your services registered in "Configure" method.
        ///     Use lifetimeScope to resolve previously registered dependencies.
        /// </summary>
        /// <param name="lifetimeScope">The AutoFac scope for resolution of registered dependencies</param>
        public void Configure(ILifetimeScope lifetimeScope);
    }
}