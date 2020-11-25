﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HtmlHelper;
using ModernSlavery.WebUI.Shared.Classes.Patterns;

namespace ModernSlavery.WebUI.Shared.Classes.TagHelpers
{
    [HtmlTargetElement("Accordion", TagStructure = TagStructure.NormalOrSelfClosing)]
    [RestrictChildren("Section")]
    public class AccordionTagHelper : TagHelper
    {
        private readonly IHtmlHelper _htmlHelper;

        public AccordionTagHelper(IHtmlHelper htmlHelper)
        {
            _htmlHelper = htmlHelper;
        }

        [ViewContext] [HtmlAttributeNotBound] public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            //Create a context for the accordion
            var accordionContext = new AccordionContext();
            context.Items.Add(typeof(AccordionContext), accordionContext);

            //Execute the sections to populate 
            await output.GetChildContentAsync();

            (_htmlHelper as IViewContextAware).Contextualize(ViewContext);

            var content = await _htmlHelper.PartialModelAsync(new Accordion(accordionContext.Sections.ToArray()));

            output.Content.SetHtmlContent(content);
            output.TagName = null;
        }
    }

    [HtmlTargetElement("Section", ParentTag = "Accordion", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class AccordionSectionTagHelper : TagHelper
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var htmlContent = await output.GetChildContentAsync();
            var accordionContext = (AccordionContext) context.Items[typeof(AccordionContext)];

            accordionContext.Sections.Add(
                new AccordionSection(Title, Description, null, Id) {HtmlContent = htmlContent});
            output.SuppressOutput();
        }
    }

    public class AccordionContext
    {
        public List<AccordionSection> Sections = new List<AccordionSection>();
    }
}