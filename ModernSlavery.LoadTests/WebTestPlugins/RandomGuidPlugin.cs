using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.WebTesting;

namespace WebPlugins
{
    public class RandomGuidPlugin : WebTestPlugin
    {
        // Properties for the plugin.  
        public string ContextParamTarget { get; set; }

        public override void PreRequest(object sender, PreRequestEventArgs e)
        {
            var guid = Guid.NewGuid().ToString().ToLower().Replace("-", "");
            e.WebTest.Context[ContextParamTarget] = guid;
        }
    }
}