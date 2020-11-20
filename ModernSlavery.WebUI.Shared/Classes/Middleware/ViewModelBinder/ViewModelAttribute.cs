using System;

namespace ModernSlavery.WebUI.Shared.Classes.Middleware.ViewModelBinder
{
    public interface IViewModelAttribute { }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ViewModelAttribute : Attribute, IViewModelAttribute
    {
        public enum StateStores
        {
            None,
            SessionStash,
            //HiddenField
        }
        public readonly StateStores StateStore;

        public ViewModelAttribute(StateStores stateStore = StateStores.None)
        {
            StateStore = stateStore;
        }
    }
}
