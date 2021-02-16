using Microsoft.VisualStudio.TestTools.WebTesting;
using System;
using System.ComponentModel;
using System.Linq;

namespace ModernSlavery.WebTestPlugins
{
    [DisplayName("Extract Attribute by random match of attribute")]
    [Description("Extract an attribute value of a random tag which matches an attribute value.")]
    public class ExtractRandomAttribute : ExtractionRule
    {
        public string TagName { get; set; }
        public string AttributeName { get; set; }
        public string MatchAttributeName { get; set; }
        public string MatchAttributeValue { get; set; }
        public bool Required { get; set; }

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

            if (Required)
            {
                // If the extraction fails, set the error text that the user sees
                e.Success = false;
                e.Message = $"No tag '{TagName}' with attribute '{MatchAttributeName}={MatchAttributeValue}'";
            }
            else
            {
                e.WebTest.Context.Remove(ContextParameterName);
                e.Success = true;
            }
        }
    }
}
