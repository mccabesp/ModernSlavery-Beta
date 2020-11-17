using System;
using Microsoft.VisualStudio.TestTools.WebTesting;

namespace ModernSlavery.WebTestPlugins
{
    public class RandomGuidPlugin : WebTestPlugin
    {
        // Properties for the plugin.  
        public string ContextParamTarget { get; set; }

        public override void PreWebTest(object sender, PreWebTestEventArgs e)
        {
            var guid = Guid.NewGuid().ToString().ToLower().Replace("-", "");
            e.WebTest.Context[ContextParamTarget] = guid;
        }

        public override void PreRequest(object sender, PreRequestEventArgs e)
        {
            base.PreRequest(sender, e);
        }
    }
}