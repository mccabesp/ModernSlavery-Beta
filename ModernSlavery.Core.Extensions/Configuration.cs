using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ModernSlavery.Core.Extensions
{
    public static partial class Extensions
    {
        public static T GetValueOrDefault<T>(this IConfiguration config, string key, T defaultValue = default)
        {
            var value = config[key];
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;
            return config.GetValue<T>(key, defaultValue);
        }

        public static string GetApplicationName(this IConfiguration configuration)
        {
            return configuration.GetValueOrDefault(HostDefaults.ApplicationKey,AppDomain.CurrentDomain.FriendlyName);
        }
        public static bool HasChildren(this IConfigurationSection section)
        {
            return section.GetChildren().Any();
        }

        public static Dictionary<string, string> ToDictionary(this IConfiguration config, string sectionName = null, bool ignoreEmpty = false)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(sectionName))
            {
                foreach (var childValue in config.GetChildValues())
                {
                    if (!ignoreEmpty || !string.IsNullOrWhiteSpace(childValue.Value)) result[childValue.Key] = childValue.Value;
                }
            }
            else
            {
                var section = config.GetSection(sectionName);
                foreach (var childValue in section.GetChildValues())
                {
                    if (childValue.Key == sectionName) continue;
                    if (!ignoreEmpty || !string.IsNullOrWhiteSpace(childValue.Value)) result[childValue.Key.Substring(section.Path.Length + 1)] = childValue.Value;
                }
            }
            return result;
        }

        public static IEnumerable<KeyValuePair<string, string>> GetChildValues(this IConfiguration config)
        {
            var children = config.GetChildren();

            if (children.Any())
                foreach (var child in children)
                {
                    foreach (var childResult in child.GetChildValues())
                        yield return childResult;
                }
        }

        public static IEnumerable<KeyValuePair<string, string>> GetChildValues(this IConfigurationSection parent)
        {
            var children = parent.GetChildren();

            if (!children.Any())
                yield return new KeyValuePair<string, string>(parent.Path, parent.Value);
            else
                foreach (var child in children)
                {
                    foreach (var childResult in child.GetChildValues())
                        yield return childResult;
                }
        }

        /// <summary>
        /// Resolves all variables using pattern $(key) from a configuration settings for all string properties in a list
        /// </summary>
        /// <param name="dictionary">The dictionary containing the keys and replacement values</param>
        /// <param name="text">The source text containing the variable declarations $(key)</param>
        /// <returns>The source text with all variable declarations replaced</returns>
        public static void ResolveVariables<T>(this IConfiguration configuration,IEnumerable<T> list)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.GetProperty);
            var dictionary = configuration.ToDictionary();

            foreach (var record in list)
            {
                foreach (var property in properties)
                {
                    var value = property.GetValue(record) as string;
                    if (value != null) property.SetValue(record,dictionary.ResolveVariableNames(value));
                }
            }
        }

        /// <summary>
        /// Resolves all variables using pattern $(key) from configuration
        /// </summary>
        /// <param name="dictionary">The dictionary containing the keys and replacement values</param>
        /// <returns>The source text with all variable declarations replaced</returns>
        public static IDictionary<string, string> ResolveVariableNames(this IConfiguration configuration)
        {
            var dictionary = configuration.ToDictionary();
            var badKeys = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            var keyStack = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var key in dictionary.Keys.ToList())
                ResolveVariableNames(dictionary[key], key);

            string ResolveVariableNames(string text, string key)
            {
                if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
                if (keyStack.Contains(key)) throw new Exception($"Circular configuration variable '$({key})'");

                if (!string.IsNullOrWhiteSpace(text))
                {
                    keyStack.Add(key);
                    var newText = text;
                    foreach (Match m in Text.VariableRegex.Matches(text))
                    {
                        var varName = m.Groups[1].Value;

                        if (dictionary.ContainsKey(varName))
                        {
                            var replacementValue = ResolveVariableNames(dictionary[varName], varName);
                            newText = newText.Replace(m.Groups[0].Value, replacementValue, StringComparison.OrdinalIgnoreCase);
                        }
                        else
                            badKeys.Add(varName);
                    }

                    if (newText != text) dictionary[key] = configuration[key] = text = newText;

                    keyStack.Remove(key);
                }

                return text;
            }

            if (badKeys.Any()) throw new KeyNotFoundException($"Cannot find configuration settings '{badKeys.ToDelimitedString(", ")}'");
            return dictionary;
        }
    }
}
