using ModernSlavery.WebUI.Models.Scope;
using Moq;


namespace ModernSlavery.WebUI.Tests.TestHelpers
{
    public class ScopingViewModelHelper
    {

        public static ScopingViewModel GetScopingViewModel()
        {
            return Mock.Of<ScopingViewModel>();
        }

    }
}
