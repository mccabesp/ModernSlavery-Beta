using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel.Attributes;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.Infrastructure.Configuration
{
    public class DependencyBuilder : IDisposable, IDependencyBuilder
    {
        private string _assemblyPrefix;
        private IContainer _optionsContainer;
        public DependencyBuilder(string assemblyPrefix)
        {
            if (string.IsNullOrWhiteSpace(assemblyPrefix)) throw new ArgumentNullException(nameof(assemblyPrefix));
            _assemblyPrefix = assemblyPrefix;
            Autofac = new ContainerBuilder();
        }

        public void Dispose()
        {
            Services = null;
            Autofac = null;
            _registeredModules = null;
            _configuredModules = null;
            _container = null;
            _autoRegisteredModules = null;
            _serviceProvider = null;
            _callingModuleNames = null;
            _registeredAssemblyNames = null;
        }

        public IServiceCollection Services { get; private set; }
        public ContainerBuilder Autofac { get; private set; }
        private HashSet<Type> _configuredModules=new HashSet<Type>();

        private IContainer _container;
        private Dictionary<Type, IDependencyModule> _registeredModules;
        private SortedSet<string> _registeredAssemblyNames = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        private SortedSet<string> _autoRegisteredModules = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        private IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Assembly> _loadedAssemblies= new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

        private Queue<string> _callingModuleNames = new Queue<string>();

        public void AddServices(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));

            Autofac.Populate(Services);

            var innerBuider = new ContainerBuilder();
            innerBuider.Populate(Services);
            _optionsContainer = innerBuider.Build();

            _registeredModules = new Dictionary<Type, IDependencyModule>();

        }

        public void Build()
        {
            //Ensure domain assemblies are preloaded
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith(_assemblyPrefix, true, default))
                .ForEach(a => _loadedAssemblies[a.FullName] = a);
            RegisterModules(Assembly.GetEntryAssembly());

            Autofac.Populate(Services);
            _container = Autofac.Build();

            //Register Autofac as the service provider

            _serviceProvider = new AutofacServiceProvider(_container);

            //Pass the services to the app builder
            if (_container.TryResolve(out IApplicationBuilder appBuilder))
            {
                appBuilder.ApplicationServices = _serviceProvider;
                Services.AddSingleton(appBuilder);
            }

            Services.AddSingleton(_container);
            Services.AddSingleton(_serviceProvider);
            Services.AddSingleton(_container);
        }

        private void RegisterModules(Assembly assembly)
        {
            if (_registeredAssemblyNames.Contains(assembly.FullName)) return;
            _registeredAssemblyNames.Add(assembly.FullName);

            foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies()
                .Where(a => a.FullName.StartsWith(_assemblyPrefix, true, default)))
            {
                Assembly referencedAssembly;

                //Make sure the assembly is loaded
                if (!_loadedAssemblies.ContainsKey(referencedAssemblyName.FullName))
                {
                    referencedAssembly = Assembly.Load(referencedAssemblyName);
                    _loadedAssemblies[referencedAssemblyName.FullName] = referencedAssembly;
                }

                referencedAssembly = _loadedAssemblies[referencedAssemblyName.FullName];

                RegisterModules(referencedAssembly);
            }

            var assemblyModuleTypes = assembly.ExportedTypes.Where(p => p.IsClass && typeof(IDependencyModule).IsAssignableFrom(p)).ToList();

            foreach (var assemblyModuleType in assemblyModuleTypes)
            {
                if (_registeredModules.ContainsKey(assemblyModuleType)) continue;

                //Check if the module is to be automatically registered if in a referenced assembly
                var autoRegisterAttribute = assemblyModuleType.GetCustomAttribute<AutoRegisterAttribute>() ?? throw new Exception($"Dependency module '{assemblyModuleType.FullName}' does not contain '[AutoRegister(false)]' attribute'");

                var auto = autoRegisterAttribute != null && autoRegisterAttribute.Enabled;
                if (!auto) continue;
                var moduleName = assemblyModuleType.FullName ?? assemblyModuleType.ToString();

                //If only 1 dependency the load it 
                var moduleInstance = CreateInstance(assemblyModuleType);
                _callingModuleNames.Enqueue(moduleName);
                moduleInstance.Register(this);
                _callingModuleNames.Dequeue();
                _registeredModules[assemblyModuleType] = moduleInstance;
                _autoRegisteredModules.Add(moduleName);
            }
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


        private void RegisterModule(Type moduleType)
        {
            var moduleName = moduleType.FullName ?? moduleType.ToString();
            var callingModuleName = _callingModuleNames.Peek();
            if (_autoRegisteredModules.Contains(moduleName))throw new Exception($"Dependency module '{callingModuleName}' explicitly references automatic module '{moduleName}'. Consider removing the reference in '{callingModuleName}' or add '[AutoRegister(false)]' attribute to the module '{moduleName}'");

            //Check if the dependency module has already been registered
            if (_registeredModules.ContainsKey(moduleType)) return;

            //Check if the module is to be automatically registered if in a referenced assembly
            var autoRegisterAttribute=moduleType.GetCustomAttribute<AutoRegisterAttribute>();
            var auto = autoRegisterAttribute != null && autoRegisterAttribute.Enabled;

            //This shouldnt happen since if the module contains a reference to a module in a referenced assembly then that module should already be registered
            if (auto) throw new Exception($"Attempt to register an automatically registered dependency module. Change '[AutoRegister(false)]' attribute for the module '{moduleName}'");

            var moduleInstance = CreateInstance(moduleType);
            moduleInstance.Register(this);
            _registeredModules[moduleType] = moduleInstance;
        }


        IDependencyModule CreateInstance(Type moduleType)
        {
            using var innerScope = _optionsContainer.BeginLifetimeScope(b => b.RegisterType(moduleType).ExternallyOwned());
            var module = (IDependencyModule)innerScope.Resolve(moduleType);
            _registeredModules[moduleType] = module;
            return module;
        }


        private void ConfigureModule(Type moduleType)
        {
            if (_configuredModules.Contains(moduleType)) return;

            if (_registeredModules == null || !_registeredModules.ContainsKey(moduleType)) throw new Exception($"Dependency module '{moduleType.FullName}' has not been registered");

            var module = _registeredModules[moduleType];

            module.Configure(_serviceProvider, _container);

            _configuredModules.Add(moduleType);
        }

        /// <summary>
        ///     Configures all previously registered dependencies declared within the Register method of the specified
        ///     IDependencyModule
        /// </summary>
        public void ConfigureModules()
        {
            if (_container==null)Build();
            if (_registeredModules == null || !_registeredModules.Any())throw new Exception("No dependency modules have been registered");

            //Resolve a new instance of the dependencies module
            foreach (var moduleType in _registeredModules.Keys.Except(_configuredModules))
                ConfigureModule(moduleType);
        }
    }
}