﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using IContainer = Autofac.IContainer;

namespace ModernSlavery.Infrastructure.Configuration
{
    public class DependencyBuilder : IDisposable, IDependencyBuilder
    {
        public IConfiguration Configuration { get; set; }
        private string _assemblyPrefix;
        private IContainer _optionsContainer;
        private readonly bool _autoConfigureOnBuild;
        public DependencyBuilder(IConfiguration configuration, string assemblyPrefix = null, bool autoConfigureOnBuild=true)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            if (string.IsNullOrWhiteSpace(assemblyPrefix)) assemblyPrefix = nameof(ModernSlavery);
            _assemblyPrefix = assemblyPrefix;
            _autoConfigureOnBuild = autoConfigureOnBuild;
        }

        public void Dispose()
        {
            Services = null;
            Autofac = null;
            _registeredModules = null;
            _configuredModules = null;
            _registeredAssemblyNames = null;
        }

        public IServiceCollection Services { get; set; }=new ServiceCollection();

        public ContainerBuilder Autofac { get; set; }
        public ILifetimeScope LifetimeScope { get; set; }
        public AutofacServiceProviderFactory ServiceProviderFactory { get; set; }

        private readonly Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
        private SortedSet<string> _registeredAssemblyNames = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<Type, IDependencyModule> _registeredModules = new Dictionary<Type, IDependencyModule>();
        private HashSet<Type> _configuredModules = new HashSet<Type>();

        public void Build<TStartupModule>(ContainerBuilder builder) where TStartupModule : class, IDependencyModule
        {
            //Create the builder for registering all dependency modules
            Autofac = builder;
            Autofac.RegisterBuildCallback(lifeTimeScope =>
            {
                LifetimeScope = lifeTimeScope;//This full scope will be required later when calling ConfigModules

                //Automatically run the configuration modules
                if (_autoConfigureOnBuild)ConfigureModules();

            });
            

            //Load all the IOptions in the domain
            var optionsBinder = new OptionsBinder(Configuration, _assemblyPrefix);
            optionsBinder.BindAssemblies();

            //Populate the temporary builder for resolving constructors of dependency modules
            var innerBuider = new ContainerBuilder();
            innerBuider.Populate(Services);
            innerBuider.Populate(optionsBinder.Services);
            _optionsContainer = innerBuider.Build();
            Autofac.Populate(optionsBinder.Services);

            //Clear the services otherwise we will get duplicated IHost which cases "Server has already started" exception when host is run.
            Services = new ServiceCollection();

            //Ensure domain assemblies are preloaded
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith(_assemblyPrefix, true, default))
                .ForEach(a => _loadedAssemblies[a.FullName] = a);

            //Register the DependencyModules in the root assembly and all descendent assemblies
            RegisterModules(typeof(TStartupModule).Assembly);

            //Populate the default builder with all the services created using dependency modules by the dependency builder
            Autofac.Populate(Services);
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

                RegisterModules(referencedAssembly);
            }

            var assemblyModuleTypes = assembly.ExportedTypes.Where(p => p.IsClass && typeof(IDependencyModule).IsAssignableFrom(p)).ToList();

            foreach (var assemblyModuleType in assemblyModuleTypes)
            {
                if (_registeredModules.ContainsKey(assemblyModuleType)) continue;

                var moduleName = assemblyModuleType.FullName ?? assemblyModuleType.ToString();

                //If only 1 dependency the load it 
                var moduleInstance = CreateInstance(assemblyModuleType);
                moduleInstance.Register(this);
                _registeredModules[assemblyModuleType] = moduleInstance;
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
            //Check if the dependency module has already been registered
            if (_registeredModules.ContainsKey(moduleType)) return;

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
    }
}