using System;

namespace ModernSlavery.WebUI.Shared.Classes.ViewModelBinder
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false, Inherited = true)]
    public class SessionViewStateAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class FormViewStateAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class IncludeViewStateAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ExcludeViewStateAttribute : Attribute
    {
    }
}
