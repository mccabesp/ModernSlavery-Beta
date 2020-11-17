using System;
using Microsoft.VisualStudio.TestTools.WebTesting;
namespace ModernSlavery.WebTestPlugins
{
    public class RelativePathPlugIn : WebTestPlugin
    {
        public override void PreWebTest(object sender, PreWebTestEventArgs e)
        {
            if (!e.WebTest.Context.ContainsKey("IsNewContext")) e.WebTest.Context["LastUrl"] = null;
            base.PreWebTest(sender, e);
        }

        public override void PreRequest(object sender, PreRequestEventArgs e)
        {
            var lastUrl= e.WebTest.Context.ContainsKey("LastUrl") ? e.WebTest.Context["LastUrl"] as Uri : null;
            if (!e.Request.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase) && lastUrl != null)
                e.Request.Url = $"{lastUrl.Scheme}://{lastUrl.Authority}{e.Request.Url}";
        }

        public override void PostPage(object sender, PostPageEventArgs e)
        {
            base.PostPage(sender, e);
            if (!string.IsNullOrWhiteSpace(e.Response?.ResponseUri?.Scheme)) e.WebTest.Context["LastUrl"] = e.Response.ResponseUri;
            e.WebTest.Context["IsNewContext"] = false;
        }
    }
}