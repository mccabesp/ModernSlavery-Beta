using System.ComponentModel;
using System.Web;
using Microsoft.VisualStudio.TestTools.WebTesting;

namespace Support
{

    [DisplayName("Tag Inner Html")]
    [Description("Extracts the inner HTML from the specified HTML tag.")]
    public class ExtractTagInnerHtml : ExtractHtmlTagInnerText
    {

        [DisplayName("Html Decode")]
        [Description("Whether on not to perform HTML decoding of the extracted string.")]
        [DefaultValue(true)]
        public bool DecodeHtml { get; set; }

        public override void Extract(object sender, ExtractionEventArgs e)
        {
            base.Extract(sender, e);

            var extract = e.WebTest.Context[this.ContextParameterName] as string;

            if (DecodeHtml && !string.IsNullOrWhiteSpace(extract)) e.WebTest.Context[this.ContextParameterName] = HttpUtility.HtmlDecode(extract);
        }
    }
}