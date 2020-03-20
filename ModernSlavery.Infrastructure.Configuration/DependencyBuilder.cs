using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.SharedKernel.Extensions;
using ModernSlavery.SharedKernel.Interfaces;

namespace ModernSlavery.Infrastructure.Configuration
{
    public class DependencyBuilder
    {
        private readonly IServiceCollection _services;
        public readonly ContainerBuilder Builder;

        public DependencyBuilder(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            Builder=new ContainerBuilder();
            Builder.Populate(services);
        }

        /// <summary>
        ///     Registers all dependencies declared within the Bind method of the specified IDependencyModule
        /// </summary>
        /// <typeparam name="TModule">The IDependenciesModule class containing the dependencies</typeparam>
        public void Bind<TModule>() where TModule : class, IDependencyModule
        {
            //Resolve a new instance of the dependencies module
            Builder.BindResolvedDependencyModule<TModule>();
        }

        /// <summary>
        ///     Bind all classes implementing IDependenciesModule in all assemblies of the current AppDomain who's assembly name starts with the specified prefix
        /// </summary>
        /// <param name="assemblyPrefix">The prefix of the assembly names to search</param>
        public void BindAssemblies(string assemblyPrefix)
        {
            if (string.IsNullOrWhiteSpace(assemblyPrefix)) throw new ArgumentNullException(nameof(assemblyPrefix));

            var type = typeof(IDependencyModule);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name.StartsWith(assemblyPrefix, true, default));

            foreach (var assembly in assemblies)
                BindAssembly(assembly);
        }

        /// <summary>
        ///     Build all dependencies all classes implementing IDependenciesModule in the specified assembly 
        /// </summary>
        /// <param name="assembly"></param>
        public void BindAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            var type = typeof(IDependencyModule);

            var moduleTypes = assembly.ExportedTypes.Where(p => type.IsAssignableFrom(p));

            foreach (var moduleType in moduleTypes)
                Builder.BindResolvedDependencyModule(moduleType);
        }

        public IServiceProvider Build()
        {
            //Configure the container
            var container = Builder.Build();

            //Register Autofac as the service provider
            var serviceProvider = new AutofacServiceProvider(container);
            _services.AddSingleton(serviceProvider);

            //Register the container
            _services.AddSingleton(container);

            return container.Resolve<IServiceProvider>();
        }

        
    }
}