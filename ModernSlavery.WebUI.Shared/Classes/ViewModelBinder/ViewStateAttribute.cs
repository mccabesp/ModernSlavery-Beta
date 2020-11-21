using System;

namespace ModernSlavery.WebUI.Shared.Classes.ViewModelBinder
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ViewStateAttribute : Attribute
    {
        public enum ActionTypes
        {
            Include,
            Exclude
        }
        public readonly ActionTypes Action;

        public ViewStateAttribute(ActionTypes action = ActionTypes.Include)
        {
            Action = action;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class IncludeAttribute : ViewStateAttribute
    {
        public IncludeAttribute() : base(ActionTypes.Include) { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ExcludeAttribute : ViewStateAttribute
    {
        public ExcludeAttribute() : base(ActionTypes.Exclude) { }
    }
}
