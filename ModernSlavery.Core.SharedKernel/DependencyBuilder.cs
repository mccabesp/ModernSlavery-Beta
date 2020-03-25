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
    public class DependencyBuilder : IDisposable
    {
        public IServiceCollection Services { get; }
        public ContainerBuilder ContainerBuilder { get; }
        public IContainer Container = null;

        private IServiceProvider _serviceProvider=null;
        private Dictionary<Type, IDependencyModule> _registeredModules = null;
        private HashSet<Type> _configuredModules = null;

        public DependencyBuilder(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            ContainerBuilder = new ContainerBuilder();
            ContainerBuilder.Populate(services);
            _registeredModules = new Dictionary<Type, IDependencyModule>();
        }

        public void Dispose()
        {
            if (Container == null) return;
            _configuredModules = null;
            _registeredModules = null;
            Container = null;
        }

        public void RegisterDependencyModule<TModule>() where TModule : class, IDependencyModule
        {
            RegisterDependencyModule(typeof(TModule));
        }

        public void RegisterDependencyModule(Type dependencyType, bool autoOnly=false)
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

        public void ConfigureDependencyModule<TModule>() where TModule : class, IDependencyModule
        {
            ConfigureDependencyModule(typeof(TModule));
        }

        public void ConfigureDependencyModule(Type serviceType)
        {
            if (_configuredModules.Contains(serviceType)) return;

            var module = _registeredModules[serviceType];

            module.Configure(Container);

            _configuredModules.Add(serviceType);
        }

        /// <summary>
        ///     Registers all dependencies declared within the Bind method of the specified IDependencyModule
        /// </summary>
        /// <typeparam name="TModule">The IDependenciesModule class containing the dependencies</typeparam>
        public void Register<TModule>() where TModule : class, IDependencyModule
        {
            //Resolve a new instance of the dependencies module
            RegisterDependencyModule<TModule>();
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

        /// <summary>
        ///     Build all dependencies all classes implementing IDependenciesModule in the specified assembly 
        /// </summary>
        /// <param name="assembly"></param>
        public void RegisterAssemblyModules(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            var type = typeof(IDependencyModule);

            var moduleTypes = assembly.ExportedTypes.Where(p => type.IsAssignableFrom(p));

            foreach (var moduleType in moduleTypes)
                RegisterDependencyModule(moduleType, true);
        }

        public IServiceProvider Build()
        {
            //Configure the container
            Container = ContainerBuilder.Build();

            //Register Autofac as the service provider
            var serviceProvider = new AutofacServiceProvider(Container);
            Services.AddSingleton(serviceProvider);

            //Register the container
            Services.AddSingleton(Container);
            
            //Create the holder for the configured modules
            _configuredModules = new HashSet<Type>();
            
            //Save the service provider for later calling the configurations
            _serviceProvider = serviceProvider;

            //Return the service provides
            return Container.Resolve<IServiceProvider>();
        }

        /// <summary>
        ///     Configures all dependencies declared within the Bind method of the specified IDependencyModule
        /// </summary>
        /// <typeparam name="TModule">The IDependenciesModule class containing the dependencies</typeparam>
        public void Configure<TModule>() where TModule : class, IDependencyModule
        {
            if (_configuredModules == null) throw new Exception("Cannot configure dependency modules until after dependencies have been built");
            //Resolve a new instance of the dependencies module
            ConfigureDependencyModule<TModule>();
        }

        /// <summary>
        ///     Configures all previously registered dependencies declared within the Register method of the specified IDependencyModule
        /// </summary>
        public void ConfigureAll()
        {
            if (_configuredModules==null)throw new Exception("Cannot configure dependency modules until after dependencies have been built");
            //Resolve a new instance of the dependencies module
            foreach (var moduleType in _registeredModules.Keys.Except(_configuredModules))
                ConfigureDependencyModule(moduleType);
        }
    }
}