using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Configuration
{
    public class OptionsBinder
    {
        private readonly IConfiguration _configuration;
        public readonly IServiceCollection Services;
        private string _assemblyPrefix;

        public OptionsBinder(IConfiguration configuration, string assemblyPrefix=null)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Services=new ServiceCollection();
            if (string.IsNullOrWhiteSpace(assemblyPrefix)) assemblyPrefix=nameof(ModernSlavery);
            _assemblyPrefix = assemblyPrefix;
        }

        private readonly Dictionary<Type, object> _bindings = new Dictionary<Type, object>();
        private readonly Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

        private object Bind(Type optionsType, string configSection = null)
        {
            if (_bindings.ContainsKey(optionsType)) return _bindings[optionsType];

            var instance = Activator.CreateInstance(optionsType);
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

                var section=_configuration.GetSection(configSection);
                foreach (var childValue in section.GetChildValues())
                {
                    if (!string.IsNullOrWhiteSpace(childValue.Value))result[childValue.Key.Substring(section.Path.Length+1)] = childValue.Value;
                }
                instance = result;
            }
            else
            {
                _configuration.Bind(configSection, instance);
            }

            Services.AddSingleton(optionsType,instance);

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

            Services.AddSingleton(instance);

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
        public void BindAssemblies()
        {
            var type = typeof(IOptions);

            AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name.StartsWith(_assemblyPrefix, true, default)).ForEach(
                a => { _loadedAssemblies[a.FullName] = a; });

            foreach (var assembly in _loadedAssemblies.Values.ToList())
                BindAssembly(assembly);
        }

        /// <summary>
        ///     Bind all classes implementing IOptions in the specified assembly
        ///     Only classes with OptionsAttribute.Key will be bound.
        /// </summary>
        /// <param name="assembly"></param>
        public void BindAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            foreach (var childAssembly in GetAssemblies(assembly))
                BindOptions(childAssembly);

            BindOptions(assembly);

            void BindOptions(Assembly assembly)
            {
                var type = typeof(IOptions);

                var optionsTypes = assembly.ExportedTypes.Where(p => p.IsClass && type.IsAssignableFrom(p));

                foreach (var optionsType in optionsTypes)
                    Bind(optionsType);
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

                    foreach (var grandChildAssembly in GetAssemblies(childAssembly))
                        yield return grandChildAssembly;
                }
            }
        }
    }
}