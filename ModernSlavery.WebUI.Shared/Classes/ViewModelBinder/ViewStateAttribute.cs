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
        public enum SecureMethods
        {
            None,
            Encrypt=1,
            Obfuscate=2
        }

        public readonly ActionTypes Action;
        public readonly SecureMethods SecureMethod;

        public ViewStateAttribute(SecureMethods secureMethod= SecureMethods.None, ActionTypes action = ActionTypes.Include)
        {
            if (secureMethod != SecureMethods.None && action == ActionTypes.Exclude) throw new ArgumentOutOfRangeException(nameof(action), $"Cannot have {nameof(secureMethod)}={secureMethod} with {nameof(action)}={action}");
            Action = action;
            SecureMethod = secureMethod;
        }
        public ViewStateAttribute(ActionTypes action)
        {
            Action = action;
            SecureMethod = SecureMethods.None;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SecuredAttribute : ViewStateAttribute
    {
        public enum SecureMethods
        {
            Encrypt=1,
            Obfuscate=2
        }

        public readonly SecureMethods SecureMethod;
        public SecuredAttribute(SecureMethods secureMethod = SecureMethods.Encrypt):base((ViewStateAttribute.SecureMethods)secureMethod, ActionTypes.Include)
        {
            SecureMethod = secureMethod;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ExcludeAttribute : ViewStateAttribute
    {
        public ExcludeAttribute() : base(SecureMethods.None, ActionTypes.Exclude)
        {
        }
    }
}
