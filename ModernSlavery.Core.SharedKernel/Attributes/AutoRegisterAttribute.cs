using System;

namespace ModernSlavery.Core.SharedKernel.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoRegisterAttribute : Attribute
    {
        public readonly bool Enabled;

        public AutoRegisterAttribute(bool enabled = true)
        {
            Enabled = enabled;
        }
    }
}