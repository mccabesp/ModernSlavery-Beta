using System;
using Microsoft.VisualStudio.TestTools.WebTesting;

namespace ModernSlavery.WebTestPlugins
{
    public class RandomIntPlugin : WebTestPlugin
    {
        // Properties for the plugin.  
        public string MinParamSource { get; set; }
        public string MaxParamSource { get; set; }
        public string ContextParamTarget { get; set; }

        public override void PrePage(object sender, PrePageEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MaxParamSource)) return;

            var min = !string.IsNullOrWhiteSpace(MinParamSource) && e.WebTest.Context.ContainsKey(MinParamSource) ? Int32.Parse(e.WebTest.Context[MinParamSource].ToString()) : 1;
            if (min < 1) min = 1;

            var max = int.Parse(e.WebTest.Context[MaxParamSource].ToString());
            if (max < min) max = min;
            e.WebTest.Context[ContextParamTarget] = new Random().Next(min, max);
        }
    }
}