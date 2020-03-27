using System;

namespace ModernSlavery.WebUI.GDSDesignSystem.Models
{
    public class PageHeadingModel
    {
        public Func<object, object> ComponentAsHtml { get; set; }
        public bool IsPageHeading { get; set; }
    }
}