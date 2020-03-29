using System;

namespace ModernSlavery.WebUI.GDSDesignSystem.Interfaces
{
    public interface IHtmlText
    {
        Func<object, object> Html { get; }
        string Text { get; }
    }
}