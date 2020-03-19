using System;

namespace ModernSlavery.SharedKernel.Options
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class OptionsAttribute:Attribute
    {
        public readonly string Key;
        public OptionsAttribute(string key)
        {
            Key=key;
        }
    }
}
