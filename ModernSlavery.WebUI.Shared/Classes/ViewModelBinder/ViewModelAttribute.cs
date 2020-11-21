using System;

namespace ModernSlavery.WebUI.Shared.Classes.ViewModelBinder
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ViewModelAttribute : Attribute
    {
        public enum StateStores
        {
            None,
            SessionStash,
            ViewState
        }
        public readonly StateStores StateStore;

        public ViewModelAttribute(StateStores stateStore = StateStores.None)
        {
            StateStore = stateStore;
        }
    }
}
