using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ModernSlavery.Core.Extensions
{
    public static class Reflect
    {
        public static Dictionary<string, object> GetPropertiesDictionary(this object source,
            BindingFlags filterFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance)
        {
            return source.GetType()
                .GetProperties(filterFlags)
                .Select(prop => new {Key = prop.Name, Value = prop.GetValue(source, null)})
                // ignore null values
                .Where(prop => prop.Value != null)
                // convert to dictionary
                .ToDictionary(prop => prop.Key, prop => prop.Value);
        }

        public static R GetProperty<R>(this object obj, string property)
        {
            var value = obj.GetType().GetProperty(property);
            if (value == null) return default(R);
            return (R) value.GetValue(obj);
        }

        public static bool IsAsyncMethod(this MethodInfo method)
        {
            // Obtain the custom attribute for the method. 
            // The value returned contains the StateMachineType property. 
            // Null is returned if the attribute isn't present for the method. 
            var attrib = (AsyncStateMachineAttribute) method.GetCustomAttribute(typeof(AsyncStateMachineAttribute));

            return attrib != null;
        }

        /// <summary>
        ///     Return all local classes which implement a single interface or abstraction which is also declared within the same
        ///     assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetSingleImplementations(this Assembly assembly)
        {
            var assemblyName = assembly.GetName().ToString();
            var types = assembly.GetTypes();
            var abstractions = types.Where(t => t.IsInterface || t.IsAbstract);
            var classes = types.Except(abstractions).Where(t => t.IsClass);

            var implementations = new Dictionary<Type, int>();

            foreach (var @class in classes)
            {
                var interfaces = @class.GetInterfaces().Where(i => i.Assembly.GetName().ToString() == assemblyName)
                    .ToArray();
                var interfaceType = interfaces.FirstOrDefault();
                if (interfaceType == null || interfaces.Length > 1) continue;

                if (interfaceType.Assembly.GetName().ToString() ==
                    assemblyName //All interfaces are declared in the same assembly 
                    && abstractions.Any(a => a.IsAssignableFrom(interfaceType)))
                    //Update the tally of implementations
                    implementations[interfaceType] = implementations.ContainsKey(interfaceType)
                        ? implementations[interfaceType] + 1
                        : 1;
            }

            //Return all classes with only one implementation
            return implementations.Where(kv => kv.Value == 1).Select(kv => kv.Key);
        }
    }
}