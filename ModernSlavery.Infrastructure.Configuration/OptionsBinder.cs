using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core.Activators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Configuration
{
    public class OptionsBinder:IDisposable
    {
        private IConfiguration _configuration;
        private IServiceCollection _services = null;
        private string _assemblyPrefix = nameof(ModernSlavery);

        public OptionsBinder(IConfiguration configuration, string assemblyPrefix=null)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _services=new ServiceCollection();

            if (!string.IsNullOrWhiteSpace(assemblyPrefix)) _assemblyPrefix= assemblyPrefix;
        }


        public void Dispose()
        {
            _configuration = null;
            _services = null;
            _bindings = null;
            _loadedAssemblies = null;
        }

        private Dictionary<Type, object> _bindings = new Dictionary<Type, object>();
        private Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
        private HashSet<Type> _optionTypes = new HashSet<Type>();


        private object Bind(object instance, string configSection = null)
        {
            var optionsType = instance.GetType();
            if (_bindings.ContainsKey(optionsType)) return _bindings[optionsType];

            var raw = false;
            if (string.IsNullOrWhiteSpace(configSection))
            {
                var configSettingAttribute = instance.GetAttribute<OptionsAttribute>();
                if (configSettingAttribute==null)throw new Exception($"Missing Options attribute on class '{optionsType.Name}'");
                if (configSettingAttribute != null) {
                    configSection = configSettingAttribute.Key;
                    raw = configSettingAttribute.RawSettings;
                }
            }

            if (configSection.TrimI().EqualsI("root",""))
            {
                _configuration.Bind(instance);
            }
            else if (raw && optionsType.BaseType.IsAssignableFrom(typeof(Dictionary<string,string>)))
            {
                var result = instance as Dictionary<string, string>;

                result.AddRange(_configuration.ToDictionary(configSection));

                instance = result;
            }
            else
            {
                _configuration.Bind(configSection, instance);
            }

            _services.AddSingleton(optionsType,instance);

            _bindings[optionsType] = instance;

            return instance;
        }

        //Bind a specific configuration section to a class and register as a singleton dependency
        /// <summary>
        ///     Populates a new instance of an IOptions class from settings in a specified configuration section
        ///     then registers it as a singleton dependency.
        ///     Multiple calls with the same type always returns the first instance registered.
        /// </summary>
        /// <typeparam name="TOptions">The IOptions class to bind the setting to</typeparam>
        /// <param name="configSection">
        ///     The name of the IConfiguration section where the settings are stored or empty to load the
        ///     root configuration settings
        /// </param>
        /// <returns>An instance of the populated IOptions concrete class</returns>
        /// <returns></returns>
        public TOptions Bind<TOptions>(string configSection = null) where TOptions : class, IOptions
        {
            return (TOptions) Bind(typeof(TOptions), configSection);
        }

        /// <summary>
        ///     Populates a new instance of an IOptions class from settings in a specified configuration section
        ///     then registers it as a singleton dependency.
        ///     Multiple calls with the same type always returns the first instance registered.
        /// </summary>
        /// <typeparam name="TOptions">The IOptions class to bind the setting to</typeparam>
        /// <param name="configSection">The IConfiguration section where the settings are stored</param>
        /// <returns>An instance of the populated IOptions concrete class</returns>
        public TOptions Bind<TOptions>(IConfiguration configSection) where TOptions : class, IOptions
        {
            var optionsType = typeof(TOptions);
            if (_bindings.ContainsKey(optionsType)) return (TOptions) _bindings[optionsType];

            var instance = Activator.CreateInstance<TOptions>();

            configSection.Bind(instance);

            _services.AddSingleton(instance);

            _bindings[optionsType] = instance;

            return instance;
        }

        /// <summary>
        ///     Return a previously bound instance of an IOptions class
        /// </summary>
        /// <typeparam name="TOptions">The IOptions class to bind the setting to</typeparam>
        /// <returns>An instance of the populated IOptions concrete class or null</returns>
        public TOptions Get<TOptions>() where TOptions : class, IOptions
        {
            var optionsType = typeof(TOptions);
            return _bindings.ContainsKey(optionsType) ? (TOptions) _bindings[optionsType] : default;
        }

        /// <summary>
        ///     Bind all classes implementing IOptions in all assemblies of the current AppDomain who's assembly name starts with
        ///     the specified prefix
        ///     Only classes with OptionsAttribute.Key will be bound.
        /// </summary>
        /// <param name="assemblyPrefix"></param>
        public IServiceCollection BindAssemblies()
        {
            var type = typeof(IOptions);

            AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name.StartsWith(_assemblyPrefix, true, default)).ForEach(
                a => { _loadedAssemblies[a.FullName] = a; });

            foreach (var assembly in _loadedAssemblies.Values.ToList())
                FindAssemblyOptions(assembly);

            BindOptions();

            return _services;
        }

        public void RegisterOptions(IServiceCollection serviceCollection)
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));

            //Register the configuration options
            _services.ForEach(service=>serviceCollection.Add(service));
        }

        /// <summary>
        ///     Bind all classes implementing IOptions in the specified assembly
        ///     Only classes with OptionsAttribute.Key will be bound.
        /// </summary>
        /// <param name="assembly"></param>
        public void FindAssemblyOptions(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            foreach (var childAssembly in GetAssemblies(assembly))
                FindAssemblyOptions(childAssembly);

            FindOptions(assembly);

            void FindOptions(Assembly assembly)
            {
                var type = typeof(IOptions);

                var optionsTypes = assembly.ExportedTypes.Where(p => p.IsClass && type.IsAssignableFrom(p));

                foreach (var optionsType in optionsTypes)
                    _optionTypes.Add(optionsType);
            }

            IEnumerable<Assembly> GetAssemblies(Assembly assembly)
            {
                foreach (var child in assembly.GetReferencedAssemblies()
                    .Where(a => a.Name.StartsWith(_assemblyPrefix, true, default)))
                {
                    if (_loadedAssemblies.ContainsKey(child.FullName)) continue;
                    var childAssembly = Assembly.Load(child);
                    _loadedAssemblies[childAssembly.FullName] = childAssembly;
                    yield return childAssembly;
                }
            }
        }

        /// <summary>
        ///     Bind all classes implementing IOptions in the specified assembly
        ///     Only classes with OptionsAttribute.Key will be bound.
        /// </summary>
        /// <param name="assembly"></param>
        public void BindOptions()
        {
            //Populate the temporary builder for resolving constructors of dependency modules
            var innerBuider = new ContainerBuilder();
            innerBuider.RegisterInstance(_configuration).SingleInstance();

            foreach (var optionsType in _optionTypes)
                innerBuider.RegisterType(optionsType).SingleInstance();

            using var optionsContainer = innerBuider.Build();

            using var innerScope = optionsContainer.BeginLifetimeScope();
            foreach (var optionsType in _optionTypes)
            {
                var options = innerScope.Resolve(optionsType);
                Bind(options);
            }

            ValidateOptions();
        }

        void ValidateOptions()
        {
             Parallel.ForEach(_bindings.Values.Cast<IOptions>(), option => option.Validate());
        }

    }
}