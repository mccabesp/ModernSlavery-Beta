using System;

namespace ModernSlavery.WebUI.Shared.Classes
{

    [AttributeUsage(AttributeTargets.Class)]
    public class DefaultResourceAttribute : Attribute
    {

        public DefaultResourceAttribute(Type resourceType)
        {
            ResourceType = resourceType;
        }

        public Type ResourceType { get; set; }

    }

}
