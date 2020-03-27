using System;

namespace ModernSlavery.WebUI.GDSDesignSystem.Partials
{
    public interface IHtmlText
    {
        Func<object, object> Html { get; }
        string Text { get; }
    }
}