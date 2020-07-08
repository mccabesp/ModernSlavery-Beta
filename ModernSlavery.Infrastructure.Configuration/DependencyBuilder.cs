using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
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

        private List<Type> _moduleTypes = new List<Type>();

        private bool _serviceActionsComplete=false;
        private bool _containerActionsComplete=false;
        private bool _configActionsComplete=false;
        public void Build<TStartupModule>(IServiceCollection optionServices, IConfiguration configuration) where TStartupModule : class, IDependencyModule
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
            innerBuider.RegisterInstance(configuration).As<IConfiguration>().SingleInstance();
            using var optionsContainer = innerBuider.Build();

            
            //Register the DependencyModules in the root assembly and all descendent assemblies
            RegisterAssembly(optionsContainer, typeof(TStartupModule).Assembly);

            //Register the modules again to account for any removals
            foreach (var moduleType in _moduleTypes.ToList())
            {
                var moduleInstance = _registeredModules[moduleType];
                moduleInstance.RegisterModules(_moduleTypes);
            }

            //Add the callbacks depth first
            foreach (var moduleType in _moduleTypes)
            {
                var moduleInstance=_registeredModules[moduleType];

                _serviceActions.Add(moduleInstance.ConfigureServices);
                _containerActions.Add(moduleInstance.ConfigureContainer);
                _configActions.Add(moduleInstance.Configure);
            }
        }

        /// <summary>
        /// //Register the DependencyModules in the assembly and all descendent assemblies
        /// </summary>
        /// <param name="assembly"></param>
        private void RegisterAssembly(IContainer optionsContainer, Assembly assembly)
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
                RegisterAssembly(optionsContainer, referencedAssembly);
            }

            //Get all referenced IDependencyModule class types
            var assemblyModuleTypes = assembly.ExportedTypes.Where(p => p.IsClass && typeof(IDependencyModule).IsAssignableFrom(p)).ToList();

            //If only 1 dependency module in the assembly then load it and its dependencies
            if (assemblyModuleTypes.Count==1)RegisterModules(optionsContainer, assemblyModuleTypes);
        }

        private void RegisterModules(IContainer optionsContainer, List<Type> assemblyModuleTypes)
        {
            foreach (var assemblyModuleType in assemblyModuleTypes)
            {
                if (_registeredModules.ContainsKey(assemblyModuleType)) continue;

                //If only 1 dependency then load it 
                var moduleInstance = CreateInstance(optionsContainer, assemblyModuleType);
                _registeredModules[assemblyModuleType] = moduleInstance;

                var childModuleTypes = new List<Type>();
                moduleInstance.RegisterModules(childModuleTypes);

                var badModules = childModuleTypes.Where(m => !m.IsClass || !typeof(IDependencyModule).IsAssignableFrom(m)).Select(m=>m.Name);
                if (badModules.Any()) throw new Exception($"Registered types do not inherit from IDependencyModule: {badModules.ToDelimitedString()}");

                if (childModuleTypes.Any())RegisterModules(optionsContainer, childModuleTypes);

                childModuleTypes.ForEach(childModuleType => { if (!_moduleTypes.Contains(childModuleType)) _moduleTypes.Add(childModuleType); });
                if (!_moduleTypes.Contains(assemblyModuleType))_moduleTypes.Add(assemblyModuleType);
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

            //Register the callback on build to configure the host
            if (Container_OnBuild != null) containerBuilder.RegisterBuildCallback((lifetimeScope)=>Container_OnBuild(lifetimeScope));
        }

        public delegate void OnContainerBuildEventHandler(ILifetimeScope lifetimeScope);
        public event OnContainerBuildEventHandler Container_OnBuild;

        public void ConfigureHost(ILifetimeScope lifetimeScope)
        {
            if (lifetimeScope == null) throw new ArgumentNullException(nameof(lifetimeScope));
            if (_configActionsComplete) return;

            //Execute all the configure actions
            _configActions.ForEach(action => action(lifetimeScope));

            _configActionsComplete = true;

            //Allow self-signed https certificates
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

        }

    }
}