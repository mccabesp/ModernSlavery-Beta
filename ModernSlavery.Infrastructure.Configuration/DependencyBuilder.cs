using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.Core.SharedKernel
{
    public class DependencyBuilder : IDisposable, IDependencyBuilder
    {
        private Dictionary<Type, IDependencyModule> _registeredModules = null;
        private HashSet<Type> _configuredModules = null;
        
        public IServiceCollection ServiceCollection { get; }
        public ContainerBuilder ContainerBuilder { get; }

        private IServiceProvider _serviceProvider = null;
        private IContainer _container = null;

        public DependencyBuilder(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            ContainerBuilder = new ContainerBuilder();
            ContainerBuilder.Populate(services);
            _registeredModules = new Dictionary<Type, IDependencyModule>();
        }

        public void Dispose()
        {
            if (_container == null) return;
            _configuredModules = null;
            _registeredModules = null;
            _container = null;
        }

        private void RegisterModule(Type dependencyType, bool autoOnly=false)
        {
            //Check if the dependency module has already been registered
            if (_registeredModules.ContainsKey(dependencyType)) return;

            ContainerBuilder.Register((context, parameters) =>
            {
                var scope = context.Resolve<ILifetimeScope>();

                using (var innerScope = scope.BeginLifetimeScope(b => b.RegisterType(dependencyType).ExternallyOwned()))
                {
                    var module = (IDependencyModule) innerScope.Resolve(dependencyType, parameters);

                    if (!autoOnly || module.AutoSetup)
                    {
                        module.Register(this);
                        _registeredModules[dependencyType] = module;
                    }

                    return module;
                }
            });
        }

        /// <summary>
        ///     Registers all dependencies declared within the Bind method of the specified IDependencyModule
        /// </summary>
        /// <typeparam name="TModule">The IDependenciesModule class containing the dependencies</typeparam>
        public void RegisterModule<TModule>() where TModule : class, IDependencyModule
        {
            //Resolve a new instance of the dependencies module
            RegisterModule(typeof(TModule));
        }

        /// <summary>
        ///     Build all dependencies all classes implementing IDependenciesModule in the specified assembly 
        /// </summary>
        /// <param name="assembly"></param>
        private void RegisterAssemblyModules(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            var type = typeof(IDependencyModule);

            var moduleTypes = assembly.ExportedTypes.Where(p => type.IsAssignableFrom(p));

            foreach (var moduleType in moduleTypes)
                RegisterModule(moduleType, true);
        }

        /// <summary>
        ///     Bind all classes implementing IDependenciesModule in all assemblies of the current AppDomain who's assembly name starts with the specified prefix
        /// </summary>
        /// <param name="assemblyPrefix">The prefix of the assembly names to search</param>
        public void RegisterDomainAssemblyModules(string assemblyPrefix)
        {
            if (string.IsNullOrWhiteSpace(assemblyPrefix)) throw new ArgumentNullException(nameof(assemblyPrefix));

            var type = typeof(IDependencyModule);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name.StartsWith(assemblyPrefix, true, default));

            foreach (var assembly in assemblies)
                RegisterAssemblyModules(assembly);
        }

        private void ConfigureModule(Type serviceType)
        {
            if (_configuredModules.Contains(serviceType)) return;

            var module = _registeredModules[serviceType];

            module.Configure(_serviceProvider,_container);

            _configuredModules.Add(serviceType);
        }

        /// <summary>
        ///     Configures all dependencies declared within the Bind method of the specified IDependencyModule
        /// </summary>
        /// <typeparam name="TModule">The IDependenciesModule class containing the dependencies</typeparam>
        public void ConfigureModule<TModule>() where TModule : class, IDependencyModule
        {
            if (_configuredModules == null) throw new Exception("Cannot configure dependency modules until after dependencies have been built");
            //Resolve a new instance of the dependencies module
            ConfigureModule(typeof(TModule));
        }

        /// <summary>
        ///     Configures all previously registered dependencies declared within the Register method of the specified IDependencyModule
        /// </summary>
        public void ConfigureModules()
        {
            if (_configuredModules == null) throw new Exception("Cannot configure dependency modules until after dependencies have been built");
            //Resolve a new instance of the dependencies module
            foreach (var moduleType in _registeredModules.Keys.Except(_configuredModules))
                ConfigureModule(moduleType);
        }


        /// <summary>
        /// Builds all the registered dependencies into the service provider and autofac container
        /// Configure all the registered dependencies marked for autosetup
        /// </summary>
        /// <returns>The system service provider</returns>
        public IServiceProvider Build()
        {
            //Configure the container
            _container = ContainerBuilder.Build();

            //Register Autofac as the service provider
            var serviceProvider = new AutofacServiceProvider(_container);
            ServiceCollection.AddSingleton(serviceProvider);

            //Register the container
            ServiceCollection.AddSingleton(_container);
            
            //Create the holder for the configured modules
            _configuredModules = new HashSet<Type>();
            
            //Save the service provider for later calling the configurations
            _serviceProvider = _container.Resolve<IServiceProvider>();

            //Configure all the autosetup modules
            ConfigureModules(); ;

            //Return the service provides
            return _serviceProvider;
        }


        
    }
}