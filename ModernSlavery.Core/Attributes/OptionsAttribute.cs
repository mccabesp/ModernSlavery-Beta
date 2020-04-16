using System;

namespace ModernSlavery.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OptionsAttribute : Attribute
    {
        public readonly string Key;
        public readonly bool RawSettings;

        public OptionsAttribute(string key, bool rawSettings=false)
        {
            Key = key;
            RawSettings = rawSettings;
        }
    }
}