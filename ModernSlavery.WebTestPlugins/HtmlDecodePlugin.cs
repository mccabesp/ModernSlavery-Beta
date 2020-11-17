using System.Web;
using Microsoft.VisualStudio.TestTools.WebTesting;

namespace ModernSlavery.WebTestPlugins
{
    public class HtmlDecodePlugin : WebTestPlugin
    {
        // Properties for the plugin.  
        public string ContextParamSource { get; set; }
        public string ContextParamTarget { get; set; }

        public override void PostRequest(object sender, PostRequestEventArgs e)
        {
            var extract = e.WebTest.Context[ContextParamSource] as string;

            if (!string.IsNullOrWhiteSpace(extract))
                e.WebTest.Context[ContextParamTarget] = HttpUtility.HtmlDecode(extract);
            else
                e.WebTest.Context[ContextParamTarget] = extract;
        }
    }
}