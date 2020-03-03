using ModernSlavery.WebUI.Admin.Models;
using Moq;


namespace ModernSlavery.Tests.TestHelpers
{
    public class ManualChangesViewModelHelper
    {

        public static ManualChangesViewModel GetMock(string command, string parameters, string comment = null)
        {
            return Mock.Of<ManualChangesViewModel>(m => m.Command == command && m.Parameters == parameters && m.Comment == comment);
        }

    }
}
