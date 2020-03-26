using System;
using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace ModernSlavery.Core.SharedKernel.Interfaces
{
    /// <summary>
    /// A module for containing all required dependencies requiring registration and associated configuration code to be executed after all application dependencies have been built
    /// Any IOptions parameters in constructors will be automatically resolved for use within Register or Configure methods
    /// /// </summary>
    public interface IDependencyModule
    {
        /// <summary>
        /// Set this to true if you want the dependencies within this module to automatically be registered by the applications dependency builder
        /// If set to false the module must be explicitly registered by a call to DependencyBuilder.Register in a DependencyModule
        /// or by a call to IserviceCollection.SetupDependencies within an application host
        /// </summary>
        bool AutoSetup { get; }

        /// <summary>
        /// Place all your dependency registrations in here using builder.ContainerBuilder (Autofac IContainer) or builder.Services (IServiceCollection)
        /// You can also register other modules using builder.Register<TModule>()
        /// </summary>
        /// <param name="builder">This is the instance of DependencyBuilder for the application host</param>
        void Register(IDependencyBuilder builder);

        /// <summary>
        /// Place any configuration of your services registered in "Configure" method.
        /// Use serviceProvider or container to resolve previously registered dependencies.
        /// </summary>
        /// <param name="serviceProvider">The system container of registered dependencies</param>
        /// <param name="container">The AutoFac container of registered dependencies</param>
        public void Configure(IServiceProvider serviceProvider, IContainer container);

    }
}
