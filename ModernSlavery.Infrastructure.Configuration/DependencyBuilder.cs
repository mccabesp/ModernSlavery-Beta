using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.Core.SharedKernel
{
    public class DependencyBuilder : IDisposable, IDependencyBuilder
    {
        private string _assemblyPrefix;
        public DependencyBuilder(string assemblyPrefix)
        {
            if (string.IsNullOrWhiteSpace(assemblyPrefix)) throw new ArgumentNullException(nameof(assemblyPrefix));
            _assemblyPrefix = assemblyPrefix;
        }

        public void Dispose()
        {
            Services = null;
            Autofac = null;
            _registeredModules = null;
            _configuredModules = null;
        }

        public IServiceCollection Services { get; private set; }
        public ContainerBuilder Autofac { get; private set; }
        private HashSet<Type> _configuredModules;

        private IContainer _container;
        private Dictionary<Type, IDependencyModule> _registeredModules;

        private IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

        public void AddServices(IServiceCollection services)
        {
            Services=services ?? throw new ArgumentNullException(nameof(services));

        }
        public void AddBuilder(ContainerBuilder containerBuilder)
        {
            Autofac = containerBuilder ?? throw new ArgumentNullException(nameof(containerBuilder));
            //var builder = new ContainerBuilder();
            Autofac.Populate(Services);
            //var container= builder.Build();

            //var serviceProvider = new AutofacServiceProvider(container);
            //Services.AddSingleton(serviceProvider);
            _registeredModules = new Dictionary<Type, IDependencyModule>();
        }

        /// <summary>
        ///     Registers all dependencies declared within the Bind method of the specified IDependencyModule
        /// </summary>
        /// <typeparam name="TModule">The IDependenciesModule class containing the dependencies</typeparam>
        public void RegisterModule<TModule>() where TModule : class, IDependencyModule
        {
            //Resolve a new instance of the dependencies module
            RegisterModule(typeof(TModule),false);
        }

        private void RegisterModule(Type dependencyType, bool autoOnly = false)
        {
            //Check if the dependency module has already been registered
            if (_registeredModules.ContainsKey(dependencyType)) return;

            Autofac.RegisterType(dependencyType).AsSelf().SingleInstance();

            Autofac.RegisterBuildCallback(scope => 
            {
                using (var innerScope = scope.BeginLifetimeScope(b => b.RegisterType(dependencyType).ExternallyOwned()))
                {
                    var module = (IDependencyModule)innerScope.Resolve(dependencyType);

                    if (!autoOnly || module.AutoSetup)
                    {
                        module.Register(this);
                        _registeredModules[dependencyType] = module;
                    }
                }
            });
        }

        /// <summary>
        ///     Build all dependencies all classes implementing IDependenciesModule in the specified assembly
        /// </summary>
        /// <param name="assembly"></param>
        private void RegisterAssemblyModules(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            void BindModules(Assembly assembly)
            {
                var type = typeof(IDependencyModule);

                var moduleTypes = assembly.ExportedTypes.Where(p => p.IsClass && type.IsAssignableFrom(p));

                foreach (var moduleType in moduleTypes)
                    RegisterModule(moduleType, true);
            }

            foreach (var childAssembly in GetAssemblies(assembly))
                BindModules(childAssembly);

            BindModules(assembly);

            IEnumerable<Assembly> GetAssemblies(Assembly assembly)
            {
                foreach (var child in assembly.GetReferencedAssemblies()
                    .Where(a => a.Name.StartsWith(_assemblyPrefix, true, default)))
                {
                    if (_loadedAssemblies.ContainsKey(child.FullName)) continue;
                    var childAssembly = Assembly.Load(child);
                    _loadedAssemblies[childAssembly.FullName] = childAssembly;
                    yield return childAssembly;

                    foreach (var grandChildAssembly in GetAssemblies(childAssembly))
                        yield return grandChildAssembly;
                }
            }

        }

        /// <summary>
        ///     Bind all classes implementing IDependenciesModule in all assemblies of the current AppDomain who's assembly name
        ///     starts with the specified prefix
        /// </summary>
        public void RegisterDomainAssemblyModules()
        {

            var type = typeof(IDependencyModule);

            AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name.StartsWith(_assemblyPrefix, true, default)).ForEach(
                a => { _loadedAssemblies[a.FullName] = a; });

            foreach (var assembly in _loadedAssemblies.Values.ToList())
                RegisterAssemblyModules(assembly);
        }

        private void ConfigureModule(Type serviceType)
        {
            if (_configuredModules.Contains(serviceType)) return;

            var module = _registeredModules[serviceType];

            module.Configure(_serviceProvider, _container);

            _configuredModules.Add(serviceType);
        }

        /// <summary>
        ///     Configures all dependencies declared within the Bind method of the specified IDependencyModule
        /// </summary>
        /// <typeparam name="TModule">The IDependenciesModule class containing the dependencies</typeparam>
        public void ConfigureModule<TModule>() where TModule : class, IDependencyModule
        {
            if (_configuredModules == null)
                throw new Exception("Cannot configure dependency modules until after dependencies have been built");
            //Resolve a new instance of the dependencies module
            ConfigureModule(typeof(TModule));
        }

        /// <summary>
        ///     Configures all previously registered dependencies declared within the Register method of the specified
        ///     IDependencyModule
        /// </summary>
        public void ConfigureModules()
        {
            if (_configuredModules == null)
                throw new Exception("Cannot configure dependency modules until after dependencies have been built");
            //Resolve a new instance of the dependencies module
            foreach (var moduleType in _registeredModules.Keys.Except(_configuredModules))
                ConfigureModule(moduleType);
        }
    }
}