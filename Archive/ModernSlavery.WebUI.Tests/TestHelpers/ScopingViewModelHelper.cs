using ModernSlavery.WebUI.Submission.Models;
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
