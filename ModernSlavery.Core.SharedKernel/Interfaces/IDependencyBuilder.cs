using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace ModernSlavery.Core.SharedKernel.Interfaces
{
    public interface IDependencyBuilder
    {
        //
        IServiceCollection Services { get; }
        ContainerBuilder Autofac { get; }

        /// <summary>
        ///     Registers all dependencies declared within the Bind method of the specified IDependencyModule
        /// </summary>
        /// <typeparam name="TModule">The IDependenciesModule class containing the dependencies</typeparam>
        void RegisterModule<TModule>() where TModule : class, IDependencyModule;
    }
}