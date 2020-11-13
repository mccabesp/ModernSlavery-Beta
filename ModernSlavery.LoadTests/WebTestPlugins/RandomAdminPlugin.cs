using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.WebTesting;

namespace WebPlugins
{
    public class RandomAdminPlugin : WebTestPlugin
    {
        // Properties for the plugin.  
        public string ContextParamTarget { get; set; }

        public override void PreRequest(object sender, PreRequestEventArgs e)
        {
            var guid = Guid.NewGuid().ToString().ToLower().Replace("-", "");
            //var val=new Random().Next(1,int.MaxValue-1);
            e.WebTest.Context[ContextParamTarget] = $"GPGTEST{guid}@geo.gov.uk";
        }
    }
}