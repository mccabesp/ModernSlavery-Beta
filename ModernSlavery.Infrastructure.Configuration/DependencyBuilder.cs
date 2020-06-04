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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using IContainer = Autofac.IContainer;

namespace ModernSlavery.Infrastructure.Configuration
{
    public class DependencyBuilder : IDisposable
    {
        private string _assemblyPrefix=nameof(ModernSlavery);

        public DependencyBuilder(string assemblyPrefix = null)
        {
            if (!string.IsNullOrWhiteSpace(assemblyPrefix)) _assemblyPrefix = assemblyPrefix;
        }

        public void Dispose()
        {
            _loadedAssemblies = null;
            _registeredAssemblyNames = null;
            _registeredModules = null;

            _serviceActions = null;
            _containerActions = null;
            _configActions = null;
        }

        private Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
        private SortedSet<string> _registeredAssemblyNames = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<Type, IDependencyModule> _registeredModules = new Dictionary<Type, IDependencyModule>();

        private List<Action<IServiceCollection>> _serviceActions = new List<Action<IServiceCollection>>();
        private List<Action<ContainerBuilder>> _containerActions = new List<Action<ContainerBuilder>>();
        private List<Action<ILifetimeScope>> _configActions = new List<Action<ILifetimeScope>>();

        private bool _serviceActionsComplete=false;
        private bool _containerActionsComplete=false;
        private bool _configActionsComplete=false;

        public void Build<TStartupModule>(IServiceCollection optionServices) where TStartupModule : class, IDependencyModule
        {
            //Ensure domain assemblies are preloaded
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith(_assemblyPrefix, true, default))
                .ForEach(a => _loadedAssemblies[a.FullName] = a);

            //Add logging services to the dependency module
            optionServices.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddEventSourceLogger();
            });

            //Populate the temporary builder for resolving constructors of dependency modules
            var innerBuider = new ContainerBuilder();
            innerBuider.Populate(optionServices);
            using var optionsContainer = innerBuider.Build();

            
            //Register the DependencyModules in the root assembly and all descendent assemblies
            RegisterModules(optionsContainer, typeof(TStartupModule).Assembly);
        }

        /// <summary>
        /// //Register the DependencyModules in the assembly and all descendent assemblies
        /// </summary>
        /// <param name="assembly"></param>
        private void RegisterModules(IContainer optionsContainer, Assembly assembly)
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
                RegisterModules(optionsContainer, referencedAssembly);
            }

            //Get alll referenced IDependencyModule class types
            var assemblyModuleTypes = assembly.ExportedTypes.Where(p => p.IsClass && typeof(IDependencyModule).IsAssignableFrom(p)).ToList();

            RegisterModules(optionsContainer, assemblyModuleTypes);
        }

        private void RegisterModules(IContainer optionsContainer, List<Type> assemblyModuleTypes)
        {
            foreach (var assemblyModuleType in assemblyModuleTypes)
            {
                if (_registeredModules.ContainsKey(assemblyModuleType)) continue;

                var moduleName = assemblyModuleType.FullName ?? assemblyModuleType.ToString();

                //If only 1 dependency then load it 
                var moduleInstance = CreateInstance(optionsContainer, assemblyModuleType);
                _registeredModules[assemblyModuleType] = moduleInstance;

                var childModules = new List<Type>();
                moduleInstance.RegisterModules(childModules);

                var badModules = childModules.Where(m => !m.IsClass || !typeof(IDependencyModule).IsAssignableFrom(m)).Select(m=>m.Name);
                if (badModules.Any()) throw new Exception($"Registered types do not inherit from IDependencyModule: {badModules.ToDelimitedString()}");

                _serviceActions.Add(moduleInstance.ConfigureServices);
                _containerActions.Add(moduleInstance.ConfigureContainer);
                _configActions.Add(moduleInstance.Configure);

                if (childModules.Any())RegisterModules(optionsContainer, childModules);
            }
        }

        private IDependencyModule CreateInstance(IContainer optionsContainer, Type moduleType)
        {
            using var innerScope = optionsContainer.BeginLifetimeScope(b => b.RegisterType(moduleType).ExternallyOwned());
            var module = (IDependencyModule)innerScope.Resolve(moduleType);
            _registeredModules[moduleType] = module;
            return module;
        }

        public void RegisterDependencyServices(IServiceCollection serviceCollection)
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));
            if (_serviceActionsComplete) return;
            
            //Register the service actions
            _serviceActions.ForEach(action => action(serviceCollection));
            
            _serviceActionsComplete = true;
        }

        public void RegisterDependencyServices(ContainerBuilder containerBuilder)
        {
            if (containerBuilder == null) throw new ArgumentNullException(nameof(containerBuilder));
            if (_containerActionsComplete) return;
            
            //Register the container actions
            _containerActions.ForEach(action => action(containerBuilder));

            _containerActionsComplete = true;
        }
        public void RegisterDependencyServices(ContainerBuilder containerBuilder, bool autoConfigureOnBuild)
        {
            RegisterDependencyServices(containerBuilder);

            //Register the callback on build to configure the host
            if (autoConfigureOnBuild) containerBuilder.RegisterBuildCallback(ConfigureHost);
        }

        public void ConfigureHost(IApplicationBuilder appBuilder)
        {
            var lifetimeScope = appBuilder.ApplicationServices.GetRequiredService<ILifetimeScope>();

            //Only add the appbuildder temporarily
            using var innerScope = lifetimeScope.BeginLifetimeScope(b => b.RegisterInstance(appBuilder).SingleInstance().ExternallyOwned());
            
            ConfigureHost(innerScope);
        }

        public void ConfigureHost(ILifetimeScope lifetimeScope)
        {
            if (lifetimeScope == null) throw new ArgumentNullException(nameof(lifetimeScope));
            if (_configActionsComplete) return;

            //Execute all the configure actions
            _configActions.ForEach(action => action(lifetimeScope));

            _configActionsComplete = true;
        }

    }
}