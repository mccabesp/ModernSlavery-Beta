using System;
using Microsoft.VisualStudio.TestTools.WebTesting;

namespace ModernSlavery.WebTestPlugins
{
    public class RandomPagePlugin : WebTestPlugin
    {
        
        // Properties for the plugin.  
        public int PageSize { get; set; } = 20;
        public string RecordTotalParamSource { get; set; }
        public string RandomPageParamTarget { get; set; }

        private static Random _rand = new Random();

        public override void PrePage(object sender, PrePageEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RecordTotalParamSource) || string.IsNullOrWhiteSpace(RandomPageParamTarget)) return;
            if (!e.WebTest.Context.ContainsKey(RecordTotalParamSource)) return;
            var recordTotalParam=e.WebTest.Context[RecordTotalParamSource].ToString();
            if (string.IsNullOrWhiteSpace(recordTotalParam)) return;

            int recordTotal = int.Parse(recordTotalParam);
            if (recordTotal <= 0) throw new ArgumentOutOfRangeException(nameof(RecordTotalParamSource));

            var pageCount = recordTotal <= 0 || PageSize <= 0 ? 0: (int)Math.Ceiling((double)recordTotal / PageSize);

            var randomPageNumber = _rand.Next(1, pageCount);
            e.WebTest.Context[RandomPageParamTarget] = randomPageNumber.ToString();
        }
    }
}