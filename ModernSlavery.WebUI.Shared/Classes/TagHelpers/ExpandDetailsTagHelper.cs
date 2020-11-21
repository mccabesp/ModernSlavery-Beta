using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HtmlHelper;
using ModernSlavery.WebUI.Shared.Classes.Patterns;

namespace ModernSlavery.WebUI.Shared.Classes.TagHelpers
{
    [HtmlTargetElement("Expand", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ExpandDetailsTagHelper : TagHelper
    {
        private readonly IHtmlHelper _htmlHelper;

        public ExpandDetailsTagHelper(IHtmlHelper htmlHelper)
        {
            _htmlHelper = htmlHelper;
        }

        [ViewContext] [HtmlAttributeNotBound] public ViewContext ViewContext { get; set; }

        public string Id { get; set; }
        public string Summary { get; set; }
        public bool IsPanel { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            (_htmlHelper as IViewContextAware).Contextualize(ViewContext);

            var htmlContent = await output.GetChildContentAsync();
            var content = await _htmlHelper.PartialModelAsync(
                new Details {Id = Id, LinkText = Summary, IsPanel = IsPanel, HtmlContent = htmlContent});

            output.Content.SetHtmlContent(content);
            output.TagName = null;
        }
    }
}