using Microsoft.VisualStudio.TestTools.WebTesting;
using Microsoft.VisualStudio.TestTools.WebTesting.Rules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.WebTestPlugins
{
    [DisplayName("Extract Random Attribute Value")]
    [Description("Extract the value of an attribute from a random specified HTML tag.")]
    public class ExtractRandomAttributeValue : ExtractionRule
    {
        DisplayNameAttribute x;
        public string TagName { get; set; }
        public string AttributeName { get; set; }
        public string MatchAttributeName { get; set; }
        public string MatchAttributeValue { get; set; }

        private static Random _rand = new Random();

        // The Extract method.  The parameter e contains the web performance test context.
        //---------------------------------------------------------------------
        public override void Extract(object sender, ExtractionEventArgs e)
        {
            if (e.Response.HtmlDocument != null)
            {
                var tags = e.Response.HtmlDocument.GetFilteredHtmlTags(new string[] { TagName }).Where(t => String.Equals(t.GetAttributeValueAsString(MatchAttributeName), MatchAttributeValue, StringComparison.InvariantCultureIgnoreCase)).ToList();
                var tag = !tags.Any() ? null : tags[_rand.Next(0,tags.Count-1)];
                if (tag!=null)
                {
                    string formFieldValue = tag.GetAttributeValueAsString(AttributeName) ?? String.Empty;

                    // add the extracted value to the web performance test context
                    e.WebTest.Context[ContextParameterName]=formFieldValue;
                    e.Success = true;
                    return;
                }
            }
            // If the extraction fails, set the error text that the user sees
            e.Success = false;
            e.Message = $"No tag '{TagName}' with attribute '{MatchAttributeName}={MatchAttributeValue}'";
        }
    }
}
