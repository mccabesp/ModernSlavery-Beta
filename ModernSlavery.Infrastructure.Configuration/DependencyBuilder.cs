using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using IContainer = Autofac.IContainer;

namespace ModernSlavery.Infrastructure.Configuration
{
    public class DependencyBuilder : IDisposable
    {
        public IConfiguration Configuration { get; set; }
        private string _assemblyPrefix;
        private IContainer _optionsContainer;
        private readonly bool _autoConfigureOnBuild;
        public DependencyBuilder(string assemblyPrefix = null, bool autoConfigureOnBuild=true)
        {
            if (string.IsNullOrWhiteSpace(assemblyPrefix)) assemblyPrefix = nameof(ModernSlavery);
            _assemblyPrefix = assemblyPrefix;
            _autoConfigureOnBuild = autoConfigureOnBuild;
        }

        public void Dispose()
        {
            _serviceActions = null;
            _containerActions = null;
            _configActions = null;
            _registeredModules = null;
            _configuredModules = null;
            _registeredAssemblyNames = null;
        }

        public ILifetimeScope LifetimeScope { get; set; }

        private readonly Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
        private SortedSet<string> _registeredAssemblyNames = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<Type, IDependencyModule> _registeredModules = new Dictionary<Type, IDependencyModule>();
        private HashSet<Type> _configuredModules = new HashSet<Type>();


        private List<Action<IServiceCollection>> _serviceActions = new List<Action<IServiceCollection>>();
        private List<Action<ContainerBuilder>> _containerActions = new List<Action<ContainerBuilder>>();
        private List<Action<ILifetimeScope>> _configActions = new List<Action<ILifetimeScope>>();

        public void Build<TStartupModule>(IServiceCollection optionServices) where TStartupModule : class, IDependencyModule
        {
            //Populate the temporary builder for resolving constructors of dependency modules
            var innerBuider = new ContainerBuilder();
            innerBuider.Populate(optionServices);
            _optionsContainer = innerBuider.Build();

            //Ensure domain assemblies are preloaded
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith(_assemblyPrefix, true, default))
                .ForEach(a => _loadedAssemblies[a.FullName] = a);

            //Register the DependencyModules in the root assembly and all descendent assemblies
            RegisterModules(typeof(TStartupModule).Assembly);
        }

        /// <summary>
        /// //Register the DependencyModules in the assembly and all descendent assemblies
        /// </summary>
        /// <param name="assembly"></param>
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

                //Recursiveley load all referenced assemblies and their modules
                RegisterModules(referencedAssembly);
            }

            //Get alll referenced IDependencyModule class types
            var assemblyModuleTypes = assembly.ExportedTypes.Where(p => p.IsClass && typeof(IDependencyModule).IsAssignableFrom(p)).ToList();

            RegisterModules(assemblyModuleTypes);
        }

        private void RegisterModules(List<Type> assemblyModuleTypes)
        {
            foreach (var assemblyModuleType in assemblyModuleTypes)
            {
                if (_registeredModules.ContainsKey(assemblyModuleType)) continue;

                var moduleName = assemblyModuleType.FullName ?? assemblyModuleType.ToString();

                //If only 1 dependency then load it 
                var moduleInstance = CreateInstance(assemblyModuleType);
                _registeredModules[assemblyModuleType] = moduleInstance;

                var childModules = new List<Type>();
                moduleInstance.RegisterModules(childModules);

                var badModules = childModules.Where(m => !m.IsClass || !typeof(IDependencyModule).IsAssignableFrom(m)).Select(m=>m.Name);
                if (badModules.Any()) throw new Exception($"Registered types do not inherit from IDependencyModule: {badModules.ToDelimitedString()}");

                _serviceActions.Add(moduleInstance.ConfigureServices);
                _containerActions.Add(moduleInstance.ConfigureContainer);
                _configActions.Add(moduleInstance.Configure);

                if (childModules.Any())RegisterModules(childModules);
            }
        }

        IDependencyModule CreateInstance(Type moduleType)
        {
            using var innerScope = _optionsContainer.BeginLifetimeScope(b => b.RegisterType(moduleType).ExternallyOwned());
            var module = (IDependencyModule)innerScope.Resolve(moduleType);
            _registeredModules[moduleType] = module;
            return module;
        }


        private void ConfigureModule(ILifetimeScope lifetimeScope, Type moduleType)
        {
            if (_configuredModules.Contains(moduleType)) return;

            if (_registeredModules == null || !_registeredModules.ContainsKey(moduleType)) throw new Exception($"Dependency module '{moduleType.FullName}' has not been registered");

            var module = _registeredModules[moduleType];

            module.Configure(lifetimeScope);

            _configuredModules.Add(moduleType);
        }

        /// <summary>
        ///     Configures all previously registered dependencies declared within the Register method of the specified
        ///     IDependencyModule
        /// </summary>
        public void ConfigureModules(params object[] additionalServices)
        {
            if (LifetimeScope==null)throw new Exception($"{nameof(ConfigureModules)} can only be called after build is complete");

            if (!_registeredModules.Any()) throw new Exception("No dependency modules have been registered");
            
            using (var innerScope = LifetimeScope.BeginLifetimeScope(builder =>
            {
                foreach (var additionalService in additionalServices)
                    builder.RegisterInstance(additionalService).AsImplementedInterfaces().SingleInstance();
            }))
            {
                //Resolve a new instance of the dependencies module
                foreach (var moduleType in _registeredModules.Keys.Except(_configuredModules))
                    ConfigureModule(innerScope, moduleType);
            }
        }

        public void PopulateHostServices(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostBuilderContext, serviceCollection) =>
            {
                //Add the services loaded from the dependency modules
            });
        }

        public void PopulateHostServices(IWebHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostBuilderContext, serviceCollection) =>
            {
                //Register the service actions
                _serviceActions.ForEach(action => action(serviceCollection));
            });
        }


        public void PopulateHostContainer(IHostBuilder hostBuilder,bool autoConfigure=false)
        {
            hostBuilder.ConfigureContainer<ContainerBuilder>((hostBuilderContext, containerBuilder) =>
            {
                //Register the container actions
                _containerActions.ForEach(action => action(containerBuilder));

                if (autoConfigure) containerBuilder.RegisterBuildCallback((lifetimeScope) => _configActions.ForEach(action => action(lifetimeScope)));
            });
        }

        public void ConfigureHost(IWebHostBuilder hostBuilder, ILifetimeScope lifetimeScope=null)
        {
            hostBuilder.Configure((appBuilder) =>
            {
                //Register the configuration actions
                lifetimeScope = lifetimeScope ?? appBuilder.ApplicationServices.GetRequiredService<ILifetimeScope>();

                _configActions.ForEach(action => action(lifetimeScope));
            });
        }

    }
}