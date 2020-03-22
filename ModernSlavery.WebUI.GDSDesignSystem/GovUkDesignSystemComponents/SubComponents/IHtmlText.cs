using System;

namespace ModernSlavery.WebUI.GDSDesignSystem.GovUkDesignSystemComponents.SubComponents
{
    public interface IHtmlText
    {

        Func<object, object> Html { get; }
        string Text { get; }

    }
}
