using System;
using Microsoft.VisualStudio.TestTools.WebTesting;

namespace ModernSlavery.WebTestPlugins
{
    public class RandomEmailPlugin : WebTestPlugin
    {
        // Properties for the plugin.  
        public string ContextParamTarget { get; set; }

        public override void PreRequest(object sender, PreRequestEventArgs e)
        {
            var guid = Guid.NewGuid().ToString().ToLower().Replace("-", "");
            //var val=new Random().Next(1,int.MaxValue-1);
            e.WebTest.Context[ContextParamTarget] = $"TISCTEST{guid}@domain.co.uk";
        }
    }
}