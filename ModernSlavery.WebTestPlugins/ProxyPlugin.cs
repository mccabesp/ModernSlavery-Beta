using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Microsoft.VisualStudio.TestTools.WebTesting;

namespace ModernSlavery.WebTestPlugins
{
    public class ProxyPlugin : WebTestPlugin
    {
        public bool Enabled { get; set; } = false;

        [DisplayName("Proxy Address")]
        [Description("The IP address and port of the proxy")]
        public string ContextParamSource { get; set; }

        //
        // Summary:
        //     Gets or sets a value that indicates whether to bypass the proxy server for local
        //     addresses.
        //
        // Returns:
        //     
        [DisplayName("Bypass local addresses")]
        [Description("Gets or sets a value that indicates whether to bypass the proxy server for local addresses.")]
        public bool BypassOnLocal { get; set; } = false;

        public override void PreWebTest(object sender, PreWebTestEventArgs e)
        {
            if (!Enabled) return;
            var proxy=string.IsNullOrWhiteSpace(ContextParamSource) || !e.WebTest.Context.ContainsKey(ContextParamSource) ? null : e.WebTest.Context[ContextParamSource].ToString();
            if (string.IsNullOrWhiteSpace(proxy)) return;
            e.WebTest.WebProxy = new WebProxy(proxy,BypassOnLocal);
        }

    }
}