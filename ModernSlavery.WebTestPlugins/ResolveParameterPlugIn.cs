using System;
using Microsoft.VisualStudio.TestTools.WebTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ModernSlavery.WebTestPlugins
{
    public class ResolveParameterPlugIn : WebTestPlugin
    {
        // Properties for the plugin.  
        private Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private const string variablePattern = @"\{\{(.*?)\}\}";
        private readonly Regex VariableRegex = new Regex(variablePattern, RegexOptions.IgnoreCase);

        public override void PreRequest(object sender, PreRequestEventArgs e)
        {
            //Resolve the context parameters
            ResetContextParameters(e);

            //Resolve the querystring parameters
            e.Request.Url = ResolveParameter(e.Request.Url);
            //foreach (var queryParameter in e.Request.QueryStringParameters)
            //    queryParameter.Value = ResolveParameter(queryParameter.Value);

            //Resolve the form post parameters
            if (e.Request.Method == "POST")
            {
                var formParameters = e.Request.Body as FormPostHttpBody;
                if (formParameters!=null)
                    foreach (var formParameter in formParameters.FormPostParameters)
                        formParameter.Value = ResolveParameter(formParameter.Value);
            }
        }

        private void ResetContextParameters(PreRequestEventArgs e)
        {
            dictionary.Clear();
            foreach (var key in e.WebTest.Context.Keys)
                dictionary[key] = ResolveParameter(e.WebTest.Context[key], true);

            foreach (var key in dictionary.Keys.ToList())
                dictionary[key] = ResolveParameter(dictionary[key], true);
        }

        private string ResolveParameter(object parameter, bool ignoreBad = false)
        {
            if (parameter == null) return null;
            var text = parameter as string;
            if (string.IsNullOrWhiteSpace(text)) return text;

            return ResolveVariable(text, ignoreBad);
        }

        private string ResolveVariable(string text, bool ignoreBad = false)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            if (string.IsNullOrWhiteSpace(text)) return text;
            var badKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match m in VariableRegex.Matches(text))
            {
                var key = m.Groups[1].Value;
                if (dictionary.ContainsKey(key))
                    text = text.Replace(m.Groups[0].Value, dictionary[key]);
                else if (!ignoreBad)
                    badKeys.Add(key);
            }

            if (badKeys.Any()) throw new KeyNotFoundException($"Cannot find variables '{string.Join(", ", badKeys)}'");
            return text;
        }
    }
}
