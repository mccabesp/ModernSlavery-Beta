using System;
using Microsoft.VisualStudio.TestTools.WebTesting;

namespace ModernSlavery.WebTestPlugins
{
    public class RandomGuidPlugin : WebTestPlugin
    {
        // Properties for the plugin.  
        public string ContextParamTarget { get; set; }
        public bool PerContext { get; set; } = true;

        public override void PreWebTest(object sender, PreWebTestEventArgs e)
        {
            if (PerContext && e.WebTest.Context.ContainsKey(ContextParamTarget) && !string.IsNullOrWhiteSpace(e.WebTest.Context[ContextParamTarget]?.ToString())) return;

            var guid = Guid.NewGuid().ToString().ToLower().Replace("-", "");
            e.WebTest.Context[ContextParamTarget] = guid;
        }
    }
}