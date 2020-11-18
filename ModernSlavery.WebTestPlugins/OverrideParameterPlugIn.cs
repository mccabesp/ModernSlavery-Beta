using System;
using Microsoft.VisualStudio.TestTools.WebTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ModernSlavery.WebTestPlugins
{
    public class OverrideParameterPlugIn : WebTestPlugin
    {
        public string SourceParam { get; set; }
        public string TargetParam { get; set; }
        public bool NoRestore { get; set; }

        private object OldValue=null;

        public override void PreWebTest(object sender, PreWebTestEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SourceParam) || string.IsNullOrWhiteSpace(TargetParam)) return;
            OldValue=e.WebTest.Context.ContainsKey(TargetParam) ? e.WebTest.Context[TargetParam] : null;
            if (e.WebTest.Context.ContainsKey(SourceParam))
                e.WebTest.Context[TargetParam] = e.WebTest.Context[SourceParam];
        }

        public override void PostWebTest(object sender, PostWebTestEventArgs e)
        {
            if (NoRestore || string.IsNullOrWhiteSpace(SourceParam) || string.IsNullOrWhiteSpace(TargetParam)) return;
            e.WebTest.Context[TargetParam] = OldValue;
        }
    }
}
