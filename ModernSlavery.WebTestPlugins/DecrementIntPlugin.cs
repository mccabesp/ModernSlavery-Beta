using Microsoft.VisualStudio.TestTools.WebTesting;

namespace ModernSlavery.WebTestPlugins
{
    public class DecrementIntPlugin : WebTestPlugin
    {
        // Properties for the plugin.  
        public string ContextParamSource { get; set; }
        public string ContextParamTarget { get; set; }
        public bool Overwrite { get; set; } = false;

        public override void PrePage(object sender, PrePageEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ContextParamSource) || string.IsNullOrWhiteSpace(ContextParamTarget)) return;

            var source = e.WebTest.Context.ContainsKey(ContextParamSource) ? e.WebTest.Context[ContextParamSource]?.ToString() : null;
            if (string.IsNullOrWhiteSpace(source)) return;

            var target = e.WebTest.Context[ContextParamTarget]?.ToString();
            if (!Overwrite && !string.IsNullOrWhiteSpace(target)) return;

            var val = int.Parse(e.WebTest.Context[ContextParamSource]?.ToString());
            val--;
            e.WebTest.Context[ContextParamTarget] = val;
        }
    }
}