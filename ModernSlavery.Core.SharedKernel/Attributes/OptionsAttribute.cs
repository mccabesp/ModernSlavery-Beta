using System;

namespace ModernSlavery.Core.SharedKernel.Options
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OptionsAttribute : Attribute
    {
        public readonly string Key;

        public OptionsAttribute(string key)
        {
            Key = key;
        }
    }
}