using System;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    public interface IObfuscatedAttribute { }

    public class ObfuscateAttribute
        : Attribute, IObfuscatedAttribute
    { }
}
